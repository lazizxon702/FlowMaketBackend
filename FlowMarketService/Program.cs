using System.Text;
using System.Threading.RateLimiting;
using DotNetEnv;
using FlowMarketService.Data;
using FlowMarketService.Extensions;
using FlowMarketService.Infrastructure;
using FlowMarketService.Models;
using FlowMarketService.Options;
using FlowMarketService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using System.Data;
using Npgsql;

if (TryLoadDotEnv() is { } envPath)
    Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey sozlanmagan yoki juda qisqa (kamida 32 belgi). Productionda muhit o'zgaruvchisi yoki User Secrets ishlating.");
}

if (builder.Environment.IsProduction() &&
    jwt.SigningKey.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Productionda Jwt__SigningKey hali placeholder. Railway Variables ichida haqiqiy tasodifiy kalit (32+ belgi) qo‘ying.");
}

builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = ConnectionStringHelper.ResolvePostgres(builder.Configuration);
    options.UseNpgsql(cs);
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
                    "Autentifikatsiya talab qilinadi yoki token yaroqsiz.",
                    context.HttpContext.TraceIdentifier,
                    "UNAUTHORIZED"));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireRole("Admin"));
    options.AddPolicy("SellerOrAdmin", p =>
        p.RequireAssertion(c =>
            c.User.IsInRole("Seller") || c.User.IsInRole("Admin")));
});

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, JsonAuthorizationMiddlewareResultHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ApiErrorResponse(
            "Juda ko'p so'rov. Bir ozdan keyin qayta urinib ko'ring.",
            context.HttpContext.TraceIdentifier,
            "TOO_MANY_REQUESTS"));
    };
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 30;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowMarket API",
        Version = "v1",
        Description =
            "The Silk Horizon REST API. Frontend: JSON kalitlari camelCase. Swagger'da \"Authorize\" ga JWT kiriting: Bearer {accessToken}"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Authorization: Bearer {accessToken}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplicationServices();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));
}
else if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
}
else
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

var applyMigrations = builder.Configuration.GetValue(
    "Database:ApplyMigrationsOnStartup",
    builder.Environment.IsDevelopment());

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupDatabase");

    var migrationsCompletedSuccessfully = false;

    if (!applyMigrations)
    {
        logger.LogInformation("Database migrations on startup are disabled (Database:ApplyMigrationsOnStartup=false).");
    }
    else
    {
        try
        {
            await db.Database.MigrateAsync();
            migrationsCompletedSuccessfully = true;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateTable || ex.SqlState == "42P07")
        {
            logger.LogWarning(ex,
                "Migration aborted: jadval allaqachon bor (schema yoki __EFMigrationsHistory mos emas). " +
                "Toza DB yoki `dotnet ef database update` / kerak bo'lsa DB ni qayta yarating.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed on startup.");
            if (builder.Environment.IsDevelopment())
                throw;
        }
    }

    var identitySchemaReady = false;
    if (!applyMigrations)
    {
        try
        {
            identitySchemaReady = await IdentityTablesExistAsync(db);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Bazaga ulanib Identity jadvallarini tekshirib bo'lmadi; seed o'tkazilmaydi.");
        }
    }

    var shouldRunSeed = applyMigrations
        ? migrationsCompletedSuccessfully
        : identitySchemaReady;

    if (!shouldRunSeed)
    {
        if (applyMigrations && !migrationsCompletedSuccessfully)
            logger.LogWarning("Seed o'tkazilmadi: migratsiya muvaffaqiyatli yakunlanmadi.");
        else if (!applyMigrations)
            logger.LogWarning(
                "Seed o'tkazilmadi: ApplyMigrationsOnStartup=false va bazada Identity jadvallari (AspNetRoles) yo'q. " +
                "Bir marta `dotnet ef database update` qiling yoki Development uchun appsettings.Development.json da ApplyMigrationsOnStartup=true qiling.");
    }
    else
    {
        try
        {
            await DbInitializer.SeedAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seed failed on startup; API davom etadi.");
        }
    }
}

app.UseExceptionHandler();

var disableHttpsRedirection = app.Configuration.GetValue("Kestrel:DisableHttpsRedirection", false);

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment() && !disableHttpsRedirection)
    app.UseHsts();

if (!disableHttpsRedirection)
    app.UseHttpsRedirection();

app.UseSecurityHeaders();
app.UseCors();
app.UseRateLimiter();

var enableSwagger = app.Environment.IsDevelopment()
    || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowMarket v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "FlowMarketService — The Silk Horizon API",
    docs = "/swagger",
    openapiJson = "/swagger/v1/swagger.json",
    modules = new[]
    {
        "/api/auth", "/api/rewards", "/api/cart", "/api/checkout", "/api/profile", "/api/merchant",
        "/api/admin", "/api/categories", "/api/products", "/api/orders", "/api/legal/documents"
    }
}));

app.MapControllers();

// Production / konteyner: Railway `PORT` bo‘yicha tinglash.
// Development (Rider, dotnet run): `launchSettings.json` dagi http://localhost:5144 — aks holda .env dagi PORT=8080
// lokalni buzadi, shuning uchun Developmentda PORT faqat konteynerda qo‘llanadi.
var portEnv = Environment.GetEnvironmentVariable("PORT");
var inContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);
var bindRailwayPort = !string.IsNullOrWhiteSpace(portEnv)
    && (inContainer || !app.Environment.IsDevelopment());

if (bindRailwayPort)
    app.Run($"http://0.0.0.0:{portEnv!.Trim()}");
else
    app.Run();

static async Task<bool> IdentityTablesExistAsync(AppDbContext db, CancellationToken cancellationToken = default)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
        await connection.OpenAsync(cancellationToken);

    await using var command = connection.CreateCommand();
    command.CommandText =
        """
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'AspNetRoles'
        );
        """;
    var scalar = await command.ExecuteScalarAsync(cancellationToken);
    return scalar is bool b && b;
}

static string? TryLoadDotEnv()
{
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var candidates = new List<string>
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(AppContext.BaseDirectory, ".env")
    };

    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (var i = 0; i < 8 && dir is not null; i++)
    {
        candidates.Add(Path.Combine(dir.FullName, ".env"));
        dir = dir.Parent;
    }

    foreach (var relative in candidates)
    {
        var full = Path.GetFullPath(relative);
        if (!seen.Add(full) || !File.Exists(full))
            continue;
        return full;
    }

    return null;
}
