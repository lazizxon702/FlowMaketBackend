using FlowMarketService.Common;
using FlowMarketService.Contracts;
using FlowMarketService.Data;
using FlowMarketService.Models;
using FlowMarketService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class MerchantService(AppDbContext db) : IMerchantService
{
    public async Task<Result<int>> SubmitApplicationAsync(Guid userId, MerchantApplyRequest request,
        CancellationToken cancellationToken = default)
    {
        var app = new MerchantApplication
        {
            ApplicantName = request.ApplicantName,
            BusinessName = request.BusinessName,
            BusinessType = request.BusinessType,
            ApplicantUserId = userId,
            Status = MerchantApplicationStatus.Pending,
            DocumentStatus = DocumentReviewStatus.Pending,
            TaxId = request.TaxId,
            SubmittedAtUtc = DateTime.UtcNow
        };
        db.MerchantApplications.Add(app);
        await db.SaveChangesAsync(cancellationToken);
        return Result<int>.Ok(app.Id, 201);
    }

    public async Task<Result<object>> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var m = await db.Merchants.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerUserId == userId, cancellationToken);
        if (m is null)
            return Result<object>.Fail("Merchant topilmadi.", 404);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var revenue = await db.OrderItems.AsNoTracking()
            .Where(oi => oi.Product.MerchantId == m.Id && oi.Order.CreatedAtUtc >= monthStart)
            .SumAsync(oi => oi.UnitPrice * oi.Quantity, cancellationToken);

        var listings = await db.Products.CountAsync(p => p.MerchantId == m.Id && p.IsActive, cancellationToken);
        var pendingOrders = await db.Orders.CountAsync(o =>
            o.Items.Any(i => i.Product.MerchantId == m.Id) && o.Status == OrderStatus.Pending, cancellationToken);

        return Result<object>.Ok(new
        {
            merchant = new { m.Id, m.Name, m.SystemCode, m.IsVerified },
            monthlyRevenueUzs = revenue,
            activeListings = listings,
            pendingOrders
        });
    }

    public async Task<Result<IReadOnlyList<object>>> GetContractsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var m = await db.Merchants.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerUserId == userId, cancellationToken);
        if (m is null)
            return Result<IReadOnlyList<object>>.Fail("Merchant topilmadi.", 404);

        var list = await db.MerchantContracts.AsNoTracking()
            .Where(c => c.MerchantId == m.Id)
            .OrderByDescending(c => c.IssuedAtUtc)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Version,
                c.Category,
                status = c.Status.ToString(),
                c.IssuedAtUtc,
                c.ExpiresAtUtc,
                c.PdfUrl,
                c.SignedAtUtc
            })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<IReadOnlyList<object>>> GetActivityAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var m = await db.Merchants.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerUserId == userId, cancellationToken);
        if (m is null)
            return Result<IReadOnlyList<object>>.Fail("Merchant topilmadi.", 404);

        var list = await db.ActivityLogs.AsNoTracking()
            .Where(a => a.MerchantId == m.Id)
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(30)
            .Select(a => new
            {
                a.Id,
                type = a.Type.ToString(),
                a.Message,
                a.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<IReadOnlyList<object>>> GetTopProductsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var m = await db.Merchants.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerUserId == userId, cancellationToken);
        if (m is null)
            return Result<IReadOnlyList<object>>.Fail("Merchant topilmadi.", 404);

        var list = await db.Products.AsNoTracking()
            .Where(p => p.MerchantId == m.Id)
            .OrderByDescending(p => p.SalesThisMonth)
            .Take(10)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.SalesThisMonth,
                p.Price,
                p.IsTrending,
                p.ImageUrl
            })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }
}
