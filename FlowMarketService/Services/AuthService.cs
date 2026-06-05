using FlowMarketService.Common;
using FlowMarketService.Contracts;
using FlowMarketService.Data;
using FlowMarketService.Models;
using FlowMarketService.Options;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FlowMarketService.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    AppDbContext db,
    JwtTokenService jwt,
    IOptions<JwtOptions> jwtOptions,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Result<AuthResponse>.Fail("Ism majburiy.");
        if (!request.AcceptTerms)
            return Result<AuthResponse>.Fail("Oferta va maxfiylik siyosatiga rozilik berilishi kerak.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Fail("Parol majburiy.");
        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            return Result<AuthResponse>.Fail("Parollar mos kelmaydi.");

        var mode = (request.Mode ?? "email").Trim().ToLowerInvariant();
        if (mode is not ("email" or "phone"))
            return Result<AuthResponse>.Fail("Mode faqat \"email\" yoki \"phone\" bo‘lishi kerak.");

        await EnsureRolesAsync(cancellationToken);

        var normalizedHandle = string.IsNullOrWhiteSpace(request.Handle) ? null : request.Handle.Trim();

        ApplicationUser user;
        if (mode == "email")
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Result<AuthResponse>.Fail("Email majburiy.");

            var email = request.Email.Trim();
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = request.FullName.Trim(),
                Handle = normalizedHandle,
                ReferralCodeOwned = await GenerateUniqueReferralCodeAsync(cancellationToken),
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        else
        {
            var phone = NormalizeUzbekPhone(request.Phone);
            if (phone is null)
                return Result<AuthResponse>.Fail("Telefon raqami noto‘g‘ri. Masalan: +998901234567 yoki 901234567.");

            if (await userManager.Users.AnyAsync(
                    u => u.PhoneNumber == phone || u.UserName == phone,
                    cancellationToken))
                return Result<AuthResponse>.Fail("Bu telefon raqami bilan akkaunt allaqachon mavjud.");

            var syntheticEmail = $"{phone.Replace("+", "", StringComparison.Ordinal)}@phone.flowmarket.local";
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = phone,
                Email = syntheticEmail,
                PhoneNumber = phone,
                PhoneNumberConfirmed = false,
                EmailConfirmed = true,
                FullName = request.FullName.Trim(),
                Handle = normalizedHandle,
                ReferralCodeOwned = await GenerateUniqueReferralCodeAsync(cancellationToken),
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        IdentityResult result;
        try
        {
            result = await userManager.CreateAsync(user, request.Password);
        }
        catch (DbUpdateException ex) when (TryMapDbUpdateException(ex, out var friendlyError))
        {
            return Result<AuthResponse>.Fail(friendlyError);
        }

        if (!result.Succeeded)
            return Result<AuthResponse>.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

        var addToRole = await userManager.AddToRoleAsync(user, "User");
        if (!addToRole.Succeeded)
            return Result<AuthResponse>.Fail(string.Join("; ", addToRole.Errors.Select(e => e.Description)));

        db.Wallets.Add(new Wallet { UserId = user.Id, CoinBalance = 0, CreditUzs = 0 });
        db.UserNotificationPreferences.Add(new UserNotificationPreferences { UserId = user.Id });
        db.UserSecurityStates.Add(new UserSecurityState { UserId = user.Id });
        db.UserTaskStates.AddRange(
            new UserTaskState { UserId = user.Id, TaskType = EarnTaskType.DailyCheckIn },
            new UserTaskState { UserId = user.Id, TaskType = EarnTaskType.WriteReview },
            new UserTaskState { UserId = user.Id, TaskType = EarnTaskType.VerifyIdentity });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryMapDbUpdateException(ex, out var friendlyError))
        {
            return Result<AuthResponse>.Fail(friendlyError);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var referrer = await db.Users.FirstOrDefaultAsync(u =>
                    u.ReferralCodeOwned == request.ReferralCode.Trim().ToUpperInvariant(),
                cancellationToken);
            if (referrer is not null && referrer.Id != user.Id)
            {
                user.ReferredByUserId = referrer.Id;
                await CreditCoinsAsync(referrer.Id, RewardConstants.ReferralBonusCoins, CoinTransactionType.ReferralBonus,
                    "Do'st taklifi uchun bonus", user.Id.ToString(), cancellationToken);
                await CreditCoinsAsync(user.Id, RewardConstants.ReferralBonusCoins, CoinTransactionType.ReferralBonus,
                    "Taklif kodi bilan ro'yxatdan o'tish", referrer.Id.ToString(), cancellationToken);
            }
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryMapDbUpdateException(ex, out var friendlyError))
        {
            return Result<AuthResponse>.Fail(friendlyError);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwt.CreateAccessToken(user.Id, user.Email!, roles);
        var refresh = Guid.NewGuid().ToString("N");
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(refresh),
            ExpiresUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedUtc = DateTime.UtcNow
        });
        await TrackActiveDeviceAsync(user.Id, cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryMapDbUpdateException(ex, out var friendlyError))
        {
            return Result<AuthResponse>.Fail(friendlyError);
        }

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        return Result<AuthResponse>.Ok(new AuthResponse(
            token,
            refresh,
            expires,
            user.Id,
            GetAuthContact(user),
            user.FullName,
            roles.ToList()));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var identifier = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(identifier))
            return Result<AuthResponse>.Fail("Email yoki telefon majburiy.", 401);
        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Fail("Parol majburiy.", 401);

        ApplicationUser? user;
        if (identifier.Contains('@', StringComparison.Ordinal))
            user = await userManager.FindByEmailAsync(identifier);
        else
        {
            var phone = NormalizeUzbekPhone(identifier);
            if (phone is null)
                return Result<AuthResponse>.Fail("Noto'g'ri email yoki parol.", 401);
            user = await userManager.Users.FirstOrDefaultAsync(
                u => u.PhoneNumber == phone || u.UserName == phone,
                cancellationToken);
        }

        if (user is null)
            return Result<AuthResponse>.Fail("Noto'g'ri email yoki parol.", 401);

        if (await userManager.IsLockedOutAsync(user))
            return Result<AuthResponse>.Fail(
                "Hisob vaqtincha bloklangan. Bir necha daqiqadan keyin qayta urinib ko‘ring.",
                401);

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthResponse>.Fail("Noto'g'ri email yoki parol.", 401);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var token = jwt.CreateAccessToken(user.Id, user.Email!, roles);
        var refresh = Guid.NewGuid().ToString("N");
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(refresh),
            ExpiresUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedUtc = DateTime.UtcNow
        });
        await TrackActiveDeviceAsync(user.Id, cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryMapDbUpdateException(ex, out var friendlyError))
        {
            return Result<AuthResponse>.Fail(friendlyError, 500);
        }

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        return Result<AuthResponse>.Ok(new AuthResponse(
            token,
            refresh,
            expires,
            user.Id,
            GetAuthContact(user),
            user.FullName,
            roles.ToList()));
    }

    public async Task<Result<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return Result<object>.Fail("Joriy parol majburiy.");
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return Result<object>.Fail("Yangi parol majburiy.");
        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
            return Result<object>.Fail("Yangi parollar mos kelmaydi.");

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return Result<object>.Fail("Foydalanuvchi topilmadi.", 404);

        var changed = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changed.Succeeded)
            return Result<object>.Fail(string.Join("; ", changed.Errors.Select(e => e.Description)));

        user.LastPasswordChangedUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await db.RefreshTokens.Where(t => t.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        return Result<object>.Ok(new { changed = true, changedAtUtc = user.LastPasswordChangedUtc });
    }

    public async Task<Result<object>> AdminResetPasswordAsync(AdminResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserIdentifier))
            return Result<object>.Fail("UserIdentifier majburiy.");
        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
            return Result<object>.Fail("Yangi parollar mos kelmaydi.");

        var identifier = request.UserIdentifier.Trim();
        ApplicationUser? user = null;

        if (Guid.TryParse(identifier, out var userId))
            user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        else if (identifier.Contains('@', StringComparison.Ordinal))
            user = await userManager.FindByEmailAsync(identifier);
        else
        {
            var phone = NormalizeUzbekPhone(identifier);
            if (phone is not null)
            {
                user = await userManager.Users.FirstOrDefaultAsync(
                    u => u.PhoneNumber == phone || u.UserName == phone,
                    cancellationToken);
            }
        }

        if (user is null)
            return Result<object>.Fail("Foydalanuvchi topilmadi.", 404);

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!reset.Succeeded)
            return Result<object>.Fail(string.Join("; ", reset.Errors.Select(e => e.Description)));

        user.LastPasswordChangedUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        await db.RefreshTokens.Where(t => t.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);

        return Result<object>.Ok(new { reset = true, userId = user.Id, changedAtUtc = user.LastPasswordChangedUtc });
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<AuthResponse>.Fail("Refresh token majburiy.", 401);

        var hash = JwtTokenService.HashRefreshToken(request.RefreshToken);
        var existing = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.ExpiresUtc > DateTime.UtcNow, cancellationToken);
        if (existing is null)
            return Result<AuthResponse>.Fail("Refresh token yaroqsiz.", 401);

        db.RefreshTokens.Remove(existing);
        var user = existing.User;
        var roles = await userManager.GetRolesAsync(user);
        var token = jwt.CreateAccessToken(user.Id, user.Email!, roles);
        var refresh = Guid.NewGuid().ToString("N");
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(refresh),
            ExpiresUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedUtc = DateTime.UtcNow
        });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryMapDbUpdateException(ex, out var friendlyError))
        {
            return Result<AuthResponse>.Fail(friendlyError, 500);
        }

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        return Result<AuthResponse>.Ok(new AuthResponse(
            token,
            refresh,
            expires,
            user.Id,
            GetAuthContact(user),
            user.FullName,
            roles.ToList()));
    }

    private static string GetAuthContact(ApplicationUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            return user.PhoneNumber;

        return user.Email ?? string.Empty;
    }

    private async Task EnsureRolesAsync(CancellationToken ct)
    {
        foreach (var r in new[] { "User", "Seller", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole<Guid>(r) { Id = Guid.NewGuid() });
        }
    }

    private async Task<string> GenerateUniqueReferralCodeAsync(CancellationToken ct)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd = Random.Shared;
        string code;
        do
        {
            code = new string(Enumerable.Range(0, 8).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        } while (await db.Users.AnyAsync(u => u.ReferralCodeOwned == code, ct));

        return code;
    }

    /// <summary>O‘zbekiston mobil: +998901234567 yoki 901234567.</summary>
    private static string? NormalizeUzbekPhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var s = raw.Trim().Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
        if (s.StartsWith('+'))
            s = s[1..];

        var digits = new string(s.Where(char.IsDigit).ToArray());
        if (digits.Length == 9 && digits[0] == '9')
            digits = "998" + digits;
        if (digits.Length != 12 || !digits.StartsWith("998", StringComparison.Ordinal))
            return null;

        return "+" + digits;
    }

    private async Task CreditCoinsAsync(Guid userId, decimal amount, CoinTransactionType type, string description,
        string? reference, CancellationToken ct)
    {
        var w = await db.Wallets.FirstAsync(x => x.UserId == userId, ct);
        w.CoinBalance += amount;
        db.CoinTransactions.Add(new CoinTransaction
        {
            UserId = userId,
            Amount = amount,
            Type = type,
            Description = description,
            Reference = reference,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private async Task TrackActiveDeviceAsync(Guid userId, CancellationToken ct)
    {
        var (deviceName, deviceType) = DetectDevice();
        var ipAddress = GetClientIpAddress();
        var locationLabel = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown location" : ipAddress;

        await db.UserDevices
            .Where(d => d.UserId == userId && d.IsCurrent)
            .ExecuteUpdateAsync(updates => updates.SetProperty(d => d.IsCurrent, false), ct);

        var existing = await db.UserDevices.FirstOrDefaultAsync(
            d => d.UserId == userId
                 && d.DeviceName == deviceName
                 && d.DeviceType == deviceType
                 && d.LocationLabel == locationLabel,
            ct);

        if (existing is null)
        {
            db.UserDevices.Add(new UserDevice
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceName = deviceName,
                DeviceType = deviceType,
                LocationLabel = locationLabel,
                LastActiveUtc = DateTime.UtcNow,
                IsCurrent = true
            });
            return;
        }

        existing.LastActiveUtc = DateTime.UtcNow;
        existing.IsCurrent = true;
    }

    private (string deviceName, string deviceType) DetectDevice()
    {
        var ua = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(ua))
            return ("Unknown device", "unknown");

        var lower = ua.ToLowerInvariant();
        var isPhone = lower.Contains("iphone", StringComparison.Ordinal)
                      || lower.Contains("android", StringComparison.Ordinal)
                      || lower.Contains("mobile", StringComparison.Ordinal);
        var isTablet = lower.Contains("ipad", StringComparison.Ordinal) || lower.Contains("tablet", StringComparison.Ordinal);
        var isMac = lower.Contains("mac os x", StringComparison.Ordinal) || lower.Contains("macintosh", StringComparison.Ordinal);
        var isWindows = lower.Contains("windows", StringComparison.Ordinal);
        var isLinux = lower.Contains("linux", StringComparison.Ordinal) && !isPhone;

        var os = isMac
            ? "Mac"
            : isWindows
                ? "Windows"
                : isLinux
                    ? "Linux"
                    : isPhone
                        ? "Phone"
                        : isTablet
                            ? "Tablet"
                            : "Unknown";
        var browser = lower.Contains("edg/", StringComparison.Ordinal)
            ? "Edge"
            : lower.Contains("chrome/", StringComparison.Ordinal)
                ? "Chrome"
                : lower.Contains("firefox/", StringComparison.Ordinal)
                    ? "Firefox"
                    : lower.Contains("safari/", StringComparison.Ordinal) && !lower.Contains("chrome/", StringComparison.Ordinal)
                        ? "Safari"
                        : "App";
        var deviceType = isPhone
            ? "phone"
            : isTablet
                ? "tablet"
                : (isMac || isWindows || isLinux)
                    ? "desktop"
                    : "unknown";

        return ($"{os} - {browser}", deviceType);
    }

    private string? GetClientIpAddress()
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
            return null;

        if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded) && forwarded.Count > 0)
        {
            var first = forwarded.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first))
                return first;
        }

        return ctx.Connection.RemoteIpAddress?.ToString();
    }

    private static bool TryMapDbUpdateException(DbUpdateException ex, out string message)
    {
        message = "Ma'lumot saqlashda xatolik yuz berdi.";

        if (ex.InnerException is not PostgresException pg || pg.SqlState != PostgresErrorCodes.UniqueViolation)
            return false;

        var constraint = pg.ConstraintName ?? string.Empty;
        if (string.Equals(constraint, "IX_AspNetUsers_NormalizedUserName", StringComparison.OrdinalIgnoreCase))
        {
            message = "Bu login (email yoki telefon) allaqachon mavjud.";
            return true;
        }

        if (string.Equals(constraint, "UserNameIndex", StringComparison.OrdinalIgnoreCase))
        {
            message = "Bu login (email yoki telefon) allaqachon mavjud.";
            return true;
        }

        if (string.Equals(constraint, "EmailIndex", StringComparison.OrdinalIgnoreCase)
            || string.Equals(constraint, "IX_AspNetUsers_NormalizedEmail", StringComparison.OrdinalIgnoreCase))
        {
            message = "Bu email allaqachon mavjud.";
            return true;
        }

        return true;
    }
}
