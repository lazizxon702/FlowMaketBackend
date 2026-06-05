using Microsoft.Extensions.Configuration;
using Npgsql;

namespace FlowMarketService.Infrastructure;

public static class ConnectionStringHelper
{
    /// <summary>
    /// <c>ConnectionStrings:DefaultConnection</c>, bo‘sh bo‘lsa yoki noto‘g‘ri bo‘lsa <c>DATABASE_URL</c> (Railway).
    /// </summary>
    public static string ResolvePostgres(IConfiguration configuration)
    {
        var explicitCs = configuration.GetConnectionString("DefaultConnection");
        var fromUrl = FromDatabaseUrl(Environment.GetEnvironmentVariable("DATABASE_URL"));

        var env = configuration["ASPNETCORE_ENVIRONMENT"] ??
                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isProd = string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);

        string? cs;
        if (isProd
            && !string.IsNullOrWhiteSpace(fromUrl)
            && !string.IsNullOrWhiteSpace(explicitCs)
            && ReferencesLocalPostgresHost(explicitCs))
        {
            // Railway: ba’zan ConnectionStrings localhost qoladi, lekin Postgres servis DATABASE_URL beradi.
            cs = fromUrl;
        }
        else if (!string.IsNullOrWhiteSpace(explicitCs))
            cs = explicitCs;
        else
            cs = fromUrl;

        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "PostgreSQL ulanishi topilmadi. Railwayda ConnectionStrings__DefaultConnection " +
                "yoki Postgres servisidan keladigan DATABASE_URL ni qo‘ying.");
        }

        if (isProd && ReferencesLocalPostgresHost(cs))
        {
            throw new InvalidOperationException(
                "Productionda PostgreSQL Host=localhost bo‘lmasligi kerak. " +
                "Railway Postgres ulanishini qo‘ying yoki DATABASE_URL ni ulang (Add variable → Reference).");
        }

        return cs;
    }

    public static string? FromDatabaseUrl(string? databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return null;

        try
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var user = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var path = uri.AbsolutePath.TrimStart('/');
            var database = Uri.UnescapeDataString(path.Split('?')[0]);

            var b = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = database,
                Username = user,
                Password = password,
                SslMode = SslMode.Require
            };
            return b.ConnectionString;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }

    private static bool ReferencesLocalPostgresHost(string connectionString)
    {
        try
        {
            var b = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrEmpty(b.Host))
                return false;
            return string.Equals(b.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(b.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return connectionString.Contains("Host=localhost", StringComparison.OrdinalIgnoreCase)
                   || connectionString.Contains("Host=127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
