using FlowMarketService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowMarketService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var r in new[] { "User", "Seller", "Admin" })
        {
            if (!await roles.RoleExistsAsync(r))
                await roles.CreateAsync(new IdentityRole<Guid>(r) { Id = Guid.NewGuid() });
        }

        if (!await db.Districts.AnyAsync(cancellationToken))
        {
            db.Districts.AddRange(
                new District { City = "Toshkent", Name = "Yunusobod" },
                new District { City = "Toshkent", Name = "Chilonzor" },
                new District { City = "Toshkent", Name = "Yashnobod" });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.ShippingOptions.AnyAsync(cancellationToken))
        {
            db.ShippingOptions.AddRange(
                new ShippingOption
                {
                    Code = "standard",
                    Name = "Standart yetkazib berish",
                    Description = "1-2 ish kuni",
                    Price = 0,
                    SortOrder = 1
                },
                new ShippingOption
                {
                    Code = "express",
                    Name = "Ekspress",
                    Description = "Shu kun 14:00 gacha buyurtma",
                    Price = 45_000,
                    SortOrder = 2
                });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Coupons.AnyAsync(cancellationToken))
        {
            db.Coupons.Add(new Coupon
            {
                Code = "SILK15",
                DiscountPercent = 15,
                ValidUntilUtc = DateTime.UtcNow.AddYears(1),
                MaxUses = 10_000
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.LegalDocuments.AnyAsync(cancellationToken))
        {
            db.LegalDocuments.AddRange(
                new LegalDocument
                {
                    Title = "Terms of Service",
                    Version = "1.0",
                    FileUrl = "/legal/tos.pdf",
                    PublishedAtUtc = DateTime.UtcNow
                },
                new LegalDocument
                {
                    Title = "Privacy Policy",
                    Version = "1.0",
                    FileUrl = "/legal/privacy.pdf",
                    PublishedAtUtc = DateTime.UtcNow
                });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (await users.FindByEmailAsync("anvar@silkh.uz") is null)
        {
            var customer = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "anvar@silkh.uz",
                Email = "anvar@silkh.uz",
                EmailConfirmed = true,
                FullName = "Anvar Alisherov",
                Handle = "anvar_uz",
                Location = "Toshkent, O'zbekiston",
                CreatedAtUtc = DateTime.UtcNow,
                ReferralCodeOwned = "ANVARUZ1"
            };
            await users.CreateAsync(customer, "Customer123!");
            await users.AddToRoleAsync(customer, "User");
            db.Wallets.Add(new Wallet { UserId = customer.Id, CoinBalance = 12_450, CreditUzs = 0 });
            db.UserNotificationPreferences.Add(new UserNotificationPreferences { UserId = customer.Id });
            db.UserSecurityStates.Add(new UserSecurityState
            {
                UserId = customer.Id,
                TwoFactorEnabled = true,
                Sms2FaEnabled = true,
                TotpEnabled = true,
                BiometricEnabled = true,
                LastSecurityCheckUtc = DateTime.UtcNow
            });
            db.UserTaskStates.AddRange(
                new UserTaskState { UserId = customer.Id, TaskType = EarnTaskType.DailyCheckIn },
                new UserTaskState { UserId = customer.Id, TaskType = EarnTaskType.WriteReview },
                new UserTaskState { UserId = customer.Id, TaskType = EarnTaskType.VerifyIdentity });
            db.UserDevices.AddRange(
                new UserDevice
                {
                    Id = Guid.NewGuid(),
                    UserId = customer.Id,
                    DeviceName = "iPhone 15 Pro",
                    DeviceType = "phone",
                    LocationLabel = "Toshkent, O'zbekiston",
                    LastActiveUtc = DateTime.UtcNow,
                    IsCurrent = true
                },
                new UserDevice
                {
                    Id = Guid.NewGuid(),
                    UserId = customer.Id,
                    DeviceName = "MacBook Air M2",
                    DeviceType = "laptop",
                    LocationLabel = "Samarqand, O'zbekiston",
                    LastActiveUtc = DateTime.UtcNow.AddHours(-2),
                    IsCurrent = false
                });
            db.UserNotifications.Add(new UserNotification
            {
                UserId = customer.Id,
                Title = "Buyurtma",
                Body = "Buyurtmangiz yo'lga chiqdi.",
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        ApplicationUser? merchantUser = await users.FindByEmailAsync("merchant@silkh.uz");
        Guid merchantId;
        if (merchantUser is null)
        {
            merchantUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "merchant@silkh.uz",
                Email = "merchant@silkh.uz",
                EmailConfirmed = true,
                FullName = "Alisher Navoiy",
                CreatedAtUtc = DateTime.UtcNow
            };
            await users.CreateAsync(merchantUser, "Merchant123!");
            await users.AddToRoleAsync(merchantUser, "Seller");
            db.Wallets.Add(new Wallet { UserId = merchantUser.Id, CoinBalance = 0, CreditUzs = 0 });
            db.UserNotificationPreferences.Add(new UserNotificationPreferences { UserId = merchantUser.Id });
            db.UserSecurityStates.Add(new UserSecurityState { UserId = merchantUser.Id });
            await db.SaveChangesAsync(cancellationToken);

            var merchant = new Merchant
            {
                Id = Guid.NewGuid(),
                Name = "Samarkand Textiles",
                SystemCode = "FM-882901",
                BusinessType = BusinessType.Textiles,
                IsVerified = true,
                OwnerUserId = merchantUser.Id,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Merchants.Add(merchant);
            await db.SaveChangesAsync(cancellationToken);
            merchantId = merchant.Id;

            db.MerchantContracts.Add(new MerchantContract
            {
                MerchantId = merchantId,
                Title = "General Service Agreement",
                Version = "v2.4",
                Category = "Service",
                Status = ContractStatus.Active,
                IssuedAtUtc = DateTime.UtcNow.AddMonths(-3),
                PdfUrl = "/contracts/gsa.pdf",
                ContentSummary = "8.5% commission cross-border, 4.2% domestic.",
                SignedAtUtc = DateTime.UtcNow.AddMonths(-2),
                SignatoryName = merchantUser.FullName,
                DigitalFingerprint = "sha256:demo"
            });

            db.ActivityLogs.AddRange(
                new ActivityLog
                {
                    MerchantId = merchantId,
                    Type = ActivityType.NewOrder,
                    Message = "New order #88219 — Silk Scarf",
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2)
                },
                new ActivityLog
                {
                    MerchantId = merchantId,
                    Type = ActivityType.StockAlert,
                    Message = "Ceramic Vase low stock",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
                });
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            merchantId = await db.Merchants.Where(m => m.OwnerUserId == merchantUser.Id).Select(m => m.Id)
                .FirstAsync(cancellationToken);
        }

        if (await users.FindByEmailAsync("admin@flow.local") is null)
        {
            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin@flow.local",
                Email = "admin@flow.local",
                EmailConfirmed = true,
                FullName = "Admin",
                CreatedAtUtc = DateTime.UtcNow
            };
            await users.CreateAsync(admin, "Admin12345!");
            await users.AddToRoleAsync(admin, "Admin");
            db.Wallets.Add(new Wallet { UserId = admin.Id, CoinBalance = 0, CreditUzs = 0 });
            db.UserNotificationPreferences.Add(new UserNotificationPreferences { UserId = admin.Id });
            db.UserSecurityStates.Add(new UserSecurityState { UserId = admin.Id });
            await db.SaveChangesAsync(cancellationToken);
        }

        await SeedConfiguredPanelAdminsAsync(users, db, config, log, cancellationToken);

        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            var electronics = new Category { Name = "Ikat & Ipak", Description = "An'anaviy matolar" };
            var ceramics = new Category { Name = "Sopol buyumlar", Description = "Rishton kulolchiligi" };
            db.Categories.AddRange(electronics, ceramics);
            await db.SaveChangesAsync(cancellationToken);

            var now = DateTime.UtcNow;
            db.Products.AddRange(
                new Product
                {
                    CategoryId = electronics.Id,
                    MerchantId = merchantId,
                    Name = "Margilan Ikat Silk",
                    Description = "Qo'lda to'qilgan",
                    AttributesSummary = "Pattern: Blue Diamond",
                    Price = 425_000m,
                    Stock = 30,
                    IsActive = true,
                    IsTrending = true,
                    SalesThisMonth = 32,
                    CreatedAtUtc = now
                },
                new Product
                {
                    CategoryId = ceramics.Id,
                    MerchantId = merchantId,
                    Name = "Rishtan Tea Set",
                    Description = "To'liq komplekt",
                    AttributesSummary = "Style: Pomegranate Blue",
                    Price = 180_000m,
                    Stock = 12,
                    IsActive = true,
                    SalesThisMonth = 18,
                    CreatedAtUtc = now
                },
                new Product
                {
                    CategoryId = ceramics.Id,
                    MerchantId = merchantId,
                    Name = "Bukhara Carved Box",
                    Description = "Yog'och",
                    AttributesSummary = "Wood: Aged Walnut",
                    Price = 310_000m,
                    Stock = 5,
                    IsActive = true,
                    CreatedAtUtc = now
                });

            await db.SaveChangesAsync(cancellationToken);
        }

        if (await db.MerchantApplications.CountAsync(cancellationToken) == 0)
        {
            var mu = await users.FindByEmailAsync("merchant@silkh.uz");
            if (mu is not null)
            {
                db.MerchantApplications.Add(new MerchantApplication
                {
                    ApplicantName = mu.FullName,
                    BusinessName = "Artisan Tech Uzbekistan",
                    BusinessType = BusinessType.Electronics,
                    ApplicantUserId = mu.Id,
                    Status = MerchantApplicationStatus.Pending,
                    DocumentStatus = DocumentReviewStatus.Pending,
                    SubmittedAtUtc = DateTime.UtcNow.AddDays(-1)
                });
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// <c>Admin:PhoneNumber</c>, <c>Admin:Email</c> (appsettings) va <c>Admin:Password</c> (masalan Development yoki
    /// <c>Admin__Password</c> muhit o‘zgaruvchisi) — admin panel uchun ikkita akkaunt. Parol Identity validatoridan
    /// mustaqil qo‘yiladi, shuning uchun soddaroq parol ham ishlashi mumkin.
    /// </summary>
    private static async Task SeedConfiguredPanelAdminsAsync(
        UserManager<ApplicationUser> users,
        AppDbContext db,
        IConfiguration config,
        ILogger log,
        CancellationToken cancellationToken)
    {
        var pwd = config["Admin:Password"];
        if (string.IsNullOrWhiteSpace(pwd))
            return;

        var phoneRaw = config["Admin:PhoneNumber"];
        var email = config["Admin:Email"]?.Trim();

        if (!string.IsNullOrWhiteSpace(phoneRaw))
        {
            var phone = NormalizeUzbekPhoneForSeed(phoneRaw.Trim());
            if (phone is null)
                log.LogWarning("Admin:PhoneNumber noto‘g‘ri format: {Phone}", phoneRaw);
            else
            {
                var synthetic = $"{phone.Replace("+", "", StringComparison.Ordinal)}@phone.flowmarket.local";
                await EnsurePanelAdminAsync(users, db, log, phone, synthetic, pwd, "Admin (telefon)",
                    cancellationToken);
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
            await EnsurePanelAdminAsync(users, db, log, phoneNumber: null, syntheticEmail: email, pwd,
                "Admin (email)", cancellationToken);
    }

    private static async Task EnsurePanelAdminAsync(
        UserManager<ApplicationUser> users,
        AppDbContext db,
        ILogger log,
        string? phoneNumber,
        string syntheticEmail,
        string password,
        string fullName,
        CancellationToken cancellationToken)
    {
        ApplicationUser? found;
        if (phoneNumber is not null)
        {
            found = await db.Users.FirstOrDefaultAsync(
                u => u.PhoneNumber == phoneNumber || u.UserName == phoneNumber || u.Email == syntheticEmail,
                cancellationToken);
        }
        else
        {
            found = await users.FindByEmailAsync(syntheticEmail);
        }

        if (found is not null)
        {
            if (!await users.IsInRoleAsync(found, "Admin"))
                await users.AddToRoleAsync(found, "Admin");
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = phoneNumber ?? syntheticEmail,
            Email = syntheticEmail,
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = phoneNumber is not null,
            EmailConfirmed = true,
            FullName = fullName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var create = await users.CreateAsync(user);
        if (!create.Succeeded)
        {
            log.LogWarning("Panel admin yaratilmadi ({Login}): {Errors}",
                phoneNumber ?? syntheticEmail,
                string.Join("; ", create.Errors.Select(e => e.Description)));
            return;
        }

        user.PasswordHash = users.PasswordHasher.HashPassword(user, password);
        user.SecurityStamp = Guid.NewGuid().ToString("D");
        var update = await users.UpdateAsync(user);
        if (!update.Succeeded)
        {
            log.LogWarning("Panel admin paroli saqlanmadi ({Login}): {Errors}",
                phoneNumber ?? syntheticEmail,
                string.Join("; ", update.Errors.Select(e => e.Description)));
            return;
        }

        await users.AddToRoleAsync(user, "Admin");
        db.Wallets.Add(new Wallet { UserId = user.Id, CoinBalance = 0, CreditUzs = 0 });
        db.UserNotificationPreferences.Add(new UserNotificationPreferences { UserId = user.Id });
        db.UserSecurityStates.Add(new UserSecurityState { UserId = user.Id });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeUzbekPhoneForSeed(string raw)
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
}
