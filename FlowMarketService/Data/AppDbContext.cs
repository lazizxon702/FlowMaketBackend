using FlowMarketService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<CoinTransaction> CoinTransactions => Set<CoinTransaction>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<MerchantApplication> MerchantApplications => Set<MerchantApplication>();
    public DbSet<MerchantContract> MerchantContracts => Set<MerchantContract>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<SavedProduct> SavedProducts => Set<SavedProduct>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserNotificationPreferences> UserNotificationPreferences => Set<UserNotificationPreferences>();
    public DbSet<UserSecurityState> UserSecurityStates => Set<UserSecurityState>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SavedPaymentMethod> SavedPaymentMethods => Set<SavedPaymentMethod>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<ShippingOption> ShippingOptions => Set<ShippingOption>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<UserTaskState> UserTaskStates => Set<UserTaskState>();
    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            e.Property(u => u.Handle).HasMaxLength(64);
            e.Property(u => u.Location).HasMaxLength(200);
            e.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            e.Property(u => u.AccountType).HasMaxLength(32);
            e.Property(u => u.ReferralCodeOwned).HasMaxLength(32);
            e.HasIndex(u => u.ReferralCodeOwned).IsUnique().HasFilter("\"ReferralCodeOwned\" IS NOT NULL");
            e.HasOne(u => u.ReferredByUser)
                .WithMany()
                .HasForeignKey(u => u.ReferredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Wallet>(e =>
        {
            e.HasKey(w => w.UserId);
            e.Property(w => w.CoinBalance).HasPrecision(18, 2);
            e.Property(w => w.CreditUzs).HasPrecision(18, 2);
            e.HasOne(w => w.User).WithOne(u => u.Wallet).HasForeignKey<Wallet>(w => w.UserId);
        });

        modelBuilder.Entity<CoinTransaction>(e =>
        {
            e.Property(c => c.Amount).HasPrecision(18, 2);
            e.Property(c => c.Description).HasMaxLength(500);
            e.Property(c => c.Reference).HasMaxLength(200);
            e.HasIndex(c => new { c.UserId, c.CreatedAtUtc });
        });

        modelBuilder.Entity<Merchant>(e =>
        {
            e.Property(m => m.Name).HasMaxLength(300);
            e.Property(m => m.SystemCode).HasMaxLength(32);
            e.HasIndex(m => m.SystemCode).IsUnique();
            e.HasOne(m => m.Owner)
                .WithMany(u => u.OwnedMerchants)
                .HasForeignKey(m => m.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MerchantApplication>(e =>
        {
            e.HasOne(a => a.Applicant)
                .WithMany(u => u.MerchantApplications)
                .HasForeignKey(a => a.ApplicantUserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Merchant)
                .WithMany(m => m.Applications)
                .HasForeignKey(a => a.MerchantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MerchantContract>(e =>
        {
            e.Property(c => c.Title).HasMaxLength(300);
            e.Property(c => c.Version).HasMaxLength(32);
            e.Property(c => c.Category).HasMaxLength(64);
        });

        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.Property(a => a.Message).HasMaxLength(1000);
            e.HasIndex(a => new { a.MerchantId, a.CreatedAtUtc });
        });

        modelBuilder.Entity<ShoppingCart>(e =>
        {
            e.HasIndex(c => c.UserId).IsUnique();
            e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.Property(i => i.UnitPriceSnapshot).HasPrecision(18, 2);
            e.Property(i => i.VariantLabel).HasMaxLength(500);
            e.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();
        });

        modelBuilder.Entity<SavedProduct>(e =>
        {
            e.HasKey(s => new { s.UserId, s.ProductId });
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Product).WithMany(p => p.SavedByUsers).HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<District>(e =>
        {
            e.Property(d => d.City).HasMaxLength(100);
            e.Property(d => d.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<UserAddress>(e =>
        {
            e.Property(a => a.Label).HasMaxLength(64);
            e.Property(a => a.Street).HasMaxLength(300);
            e.Property(a => a.HouseNumber).HasMaxLength(32);
            e.Property(a => a.Apartment).HasMaxLength(32);
            e.Property(a => a.Comment).HasMaxLength(500);
        });

        modelBuilder.Entity<UserNotificationPreferences>(e =>
        {
            e.HasKey(p => p.UserId);
            e.HasOne(p => p.User).WithOne(u => u.NotificationPreferences)
                .HasForeignKey<UserNotificationPreferences>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSecurityState>(e =>
        {
            e.HasKey(s => s.UserId);
            e.HasOne(s => s.User).WithOne(u => u.SecurityState)
                .HasForeignKey<UserSecurityState>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserDevice>(e =>
        {
            e.Property(d => d.DeviceName).HasMaxLength(200);
            e.Property(d => d.DeviceType).HasMaxLength(64);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(t => t.TokenHash);
        });

        modelBuilder.Entity<SavedPaymentMethod>(e =>
        {
            e.Property(p => p.MaskedPan).HasMaxLength(32);
            e.Property(p => p.CardholderName).HasMaxLength(200);
            e.Property(p => p.Brand).HasMaxLength(32);
        });

        modelBuilder.Entity<Coupon>(e =>
        {
            e.Property(c => c.Code).HasMaxLength(64);
            e.HasIndex(c => c.Code).IsUnique();
        });

        modelBuilder.Entity<SupportTicket>(e =>
        {
            e.Property(t => t.Subject).HasMaxLength(200);
            e.Property(t => t.Message).HasMaxLength(4000);
        });

        modelBuilder.Entity<ShippingOption>(e =>
        {
            e.Property(s => s.Code).HasMaxLength(32);
            e.Property(s => s.Name).HasMaxLength(120);
            e.Property(s => s.Description).HasMaxLength(500);
            e.Property(s => s.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<UserNotification>(e =>
        {
            e.Property(n => n.Title).HasMaxLength(200);
            e.Property(n => n.Body).HasMaxLength(2000);
            e.HasIndex(n => new { n.UserId, n.CreatedAtUtc });
        });

        modelBuilder.Entity<UserTaskState>(e =>
        {
            e.HasKey(t => new { t.UserId, t.TaskType });
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LegalDocument>(e =>
        {
            e.Property(d => d.Title).HasMaxLength(200);
            e.Property(d => d.FileUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(c => c.Name);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(300).IsRequired();
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.AttributesSummary).HasMaxLength(500);
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Merchant)
                .WithMany(m => m.Products)
                .HasForeignKey(p => p.MerchantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(o => o.Email).HasMaxLength(320).IsRequired();
            e.Property(o => o.Phone).HasMaxLength(50);
            e.Property(o => o.PromoCodeApplied).HasMaxLength(64);
            e.Property(o => o.Subtotal).HasPrecision(18, 2);
            e.Property(o => o.ShippingFee).HasPrecision(18, 2);
            e.Property(o => o.Discount).HasPrecision(18, 2);
            e.Property(o => o.Total).HasPrecision(18, 2);
            e.HasIndex(o => o.CreatedAtUtc);
            e.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(o => o.ShippingOption)
                .WithMany()
                .HasForeignKey(o => o.ShippingOptionId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(o => o.SavedPaymentMethod)
                .WithMany()
                .HasForeignKey(o => o.SavedPaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            e.Property(i => i.VariantDescription).HasMaxLength(500);
            e.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
