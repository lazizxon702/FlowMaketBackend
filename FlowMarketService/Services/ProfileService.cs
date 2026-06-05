using FlowMarketService.Common;
using FlowMarketService.Contracts.Profile;
using FlowMarketService.Data;
using FlowMarketService.Models;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class ProfileService(AppDbContext db, UserManager<ApplicationUser> users) : IProfileService
{
    public async Task<Result<object>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var u = await users.FindByIdAsync(userId.ToString());
        if (u is null)
            return Result<object>.Fail("Foydalanuvchi topilmadi.", 404);
        var roles = await users.GetRolesAsync(u);
        return Result<object>.Ok(new
        {
            u.Id,
            u.FullName,
            u.Handle,
            u.Location,
            u.ProfilePictureUrl,
            u.DateOfBirth,
            u.Email,
            u.PhoneNumber,
            u.AccountType,
            emailVerified = u.EmailConfirmed,
            u.ReferralCodeOwned,
            roles
        });
    }

    public async Task<Result<object>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var u = await users.FindByIdAsync(userId.ToString());
        if (u is null)
            return Result<object>.Fail("Foydalanuvchi topilmadi.", 404);

        if (!string.IsNullOrWhiteSpace(request.FullName))
            u.FullName = request.FullName.Trim();
        if (request.Handle is not null)
            u.Handle = string.IsNullOrWhiteSpace(request.Handle) ? null : request.Handle.Trim();
        if (request.Location is not null)
            u.Location = request.Location.Trim();
        if (request.ProfilePictureUrl is not null)
            u.ProfilePictureUrl = request.ProfilePictureUrl;
        if (request.DateOfBirth is not null)
            u.DateOfBirth = request.DateOfBirth;
        if (!string.IsNullOrWhiteSpace(request.Phone))
            u.PhoneNumber = request.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(request.AccountType))
            u.AccountType = request.AccountType.Trim();

        await users.UpdateAsync(u);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<object>> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var u = await users.FindByIdAsync(userId.ToString());
        if (u is null)
            return Result<object>.Fail("Foydalanuvchi topilmadi.", 404);

        await db.RefreshTokens.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserDevices.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.SavedProducts.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserNotifications.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.SupportTickets.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserTaskStates.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserNotificationPreferences.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.UserSecurityStates.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.Wallets.Where(x => x.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        var delete = await users.DeleteAsync(u);
        if (!delete.Succeeded)
            return Result<object>.Fail(string.Join("; ", delete.Errors.Select(e => e.Description)));

        return Result<object>.Ok(new { deleted = true });
    }

    public async Task<Result<object>> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await db.Orders.CountAsync(o => o.UserId == userId, cancellationToken);
        var saved = await db.SavedProducts.CountAsync(s => s.UserId == userId, cancellationToken);
        var coupons = await db.Coupons.CountAsync(cancellationToken);
        return Result<object>.Ok(new { orders, saved, coupons });
    }

    public async Task<Result<IReadOnlyList<object>>> ListSavedProductsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var list = await db.SavedProducts.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Include(s => s.Product)
            .Select(s => new { s.ProductId, s.Product.Name, s.Product.Price, s.Product.ImageUrl })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<object>> SaveProductAsync(Guid userId, int productId,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
            return Result<object>.Fail("Mahsulot topilmadi.", 404);
        if (await db.SavedProducts.AnyAsync(s => s.UserId == userId && s.ProductId == productId, cancellationToken))
            return Result<object>.Ok(new { ok = true });
        db.SavedProducts.Add(new SavedProduct
        {
            UserId = userId,
            ProductId = productId,
            SavedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<object?>> UnsaveProductAsync(Guid userId, int productId,
        CancellationToken cancellationToken = default)
    {
        var s = await db.SavedProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId,
            cancellationToken);
        if (s is null)
            return Result<object?>.Fail("Topilmadi.", 404);
        db.SavedProducts.Remove(s);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object?>.Ok(null, 204);
    }

    public async Task<Result<IReadOnlyList<object>>> ListNotificationsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var list = await db.UserNotifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(50)
            .Select(n => new { n.Id, n.Title, n.Body, n.IsRead, n.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<object>> MarkNotificationReadAsync(Guid userId, long id,
        CancellationToken cancellationToken = default)
    {
        var n = await db.UserNotifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (n is null)
            return Result<object>.Fail("Topilmadi.", 404);
        n.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }

    public async Task<Result<int>> CreateSupportTicketAsync(Guid userId, SupportTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var t = new SupportTicket
        {
            UserId = userId,
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        db.SupportTickets.Add(t);
        await db.SaveChangesAsync(cancellationToken);
        return Result<int>.Ok(t.Id, 201);
    }

    public async Task<Result<object>> GetSecurityStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var st = await db.UserSecurityStates.AsNoTracking().FirstAsync(s => s.UserId == userId, cancellationToken);
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, cancellationToken);
        return Result<object>.Ok(new
        {
            status = "HIMOYALANGAN",
            message = "Hisobingiz xavfsiz",
            lastCheck = st.LastSecurityCheckUtc ?? DateTime.UtcNow,
            twoFactor = st.TwoFactorEnabled,
            biometric = st.BiometricEnabled,
            lastPasswordChange = user.LastPasswordChangedUtc
        });
    }

    public async Task<Result<IReadOnlyList<ActiveSessionDto>>> ListDevicesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var list = await db.UserDevices.AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastActiveUtc)
            .ToListAsync(cancellationToken);
        var cards = list.Select(MapToActiveSession).ToList();
        return Result<IReadOnlyList<ActiveSessionDto>>.Ok(cards);
    }

    public async Task<Result<object>> LogoutAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await db.RefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        return Result<object>.Ok(new { revoked = true });
    }

    public async Task<Result<object>> RemoveDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await db.UserDevices.FirstOrDefaultAsync(d => d.UserId == userId && d.Id == deviceId, cancellationToken);
        if (device is null)
            return Result<object>.Fail("Qurilma topilmadi.", 404);

        var age = DateTime.UtcNow - device.LastActiveUtc;
        if (age < TimeSpan.FromHours(24))
            return Result<object>.Fail("Qurilmani o'chirish uchun oxirgi faollikdan kamida 24 soat o'tishi kerak.");

        db.UserDevices.Remove(device);
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { removed = true, deviceId });
    }

    private static ActiveSessionDto MapToActiveSession(UserDevice d)
    {
        var location = string.IsNullOrWhiteSpace(d.LocationLabel) ? "Unknown" : d.LocationLabel!;
        var title = $"{d.DeviceName} - {location}";
        var subtitle = BuildSubtitle(d);
        var lastActivityText = BuildLastActivityText(d.LastActiveUtc);
        return new ActiveSessionDto(d.Id, title, subtitle, lastActivityText, d.LastActiveUtc, d.IsCurrent);
    }

    private static string BuildSubtitle(UserDevice d)
    {
        var appLabel = string.Equals(d.DeviceType, "phone", StringComparison.OrdinalIgnoreCase)
            ? "FlowMarket App"
            : string.Equals(d.DeviceType, "tablet", StringComparison.OrdinalIgnoreCase)
                ? "FlowMarket App"
                : "Web session";

        if (string.IsNullOrWhiteSpace(d.LocationLabel))
            return appLabel;
        return $"{appLabel} • {d.LocationLabel}";
    }

    private static string BuildLastActivityText(DateTime lastActiveUtc)
    {
        var local = lastActiveUtc.ToLocalTime();
        var today = DateTime.Now.Date;
        if (local.Date == today)
            return $"Сегодня, {local:HH:mm}";

        var days = (today - local.Date).Days;
        if (days <= 0)
            return $"Сегодня, {local:HH:mm}";
        if (days == 1)
            return "1 день назад";
        if (days is >= 2 and <= 4)
            return $"{days} дня назад";
        return $"{days} дней назад";
    }

    public async Task<Result<object>> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var p = await db.UserNotificationPreferences.AsNoTracking().FirstAsync(x => x.UserId == userId, cancellationToken);
        return Result<object>.Ok(new
        {
            p.OrderStatusEnabled,
            p.SecurityEnabled,
            p.FlashSalesEnabled,
            p.NewArrivalsEnabled,
            p.AiDigestComingSoon
        });
    }

    public async Task<Result<object>> PatchNotificationSettingsAsync(Guid userId, PatchNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var p = await db.UserNotificationPreferences.FirstAsync(x => x.UserId == userId, cancellationToken);
        if (request.OrderStatusEnabled is { } a)
            p.OrderStatusEnabled = a;
        if (request.SecurityEnabled is { } b)
            p.SecurityEnabled = b;
        if (request.FlashSalesEnabled is { } c)
            p.FlashSalesEnabled = c;
        if (request.NewArrivalsEnabled is { } d)
            p.NewArrivalsEnabled = d;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }
}
