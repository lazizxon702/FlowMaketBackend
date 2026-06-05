using FlowMarketService.Common;
using FlowMarketService.Data;
using FlowMarketService.Models;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class AdminService(AppDbContext db, UserManager<ApplicationUser> userManager) : IAdminService
{
    public async Task<Result<object>> GetMerchantApplicationStatsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await db.MerchantApplications.CountAsync(a => a.Status == MerchantApplicationStatus.Pending, cancellationToken);
        var verified = await db.MerchantApplications.CountAsync(a => a.Status == MerchantApplicationStatus.Verified, cancellationToken);
        var rejected = await db.MerchantApplications.CountAsync(a => a.Status == MerchantApplicationStatus.Rejected, cancellationToken);
        var missingTax = await db.MerchantApplications.CountAsync(a =>
            a.Status == MerchantApplicationStatus.Pending && string.IsNullOrEmpty(a.TaxId), cancellationToken);

        return Result<object>.Ok(new
        {
            queueSize = pending,
            verifiedTotal = verified,
            rejectedTotal = rejected,
            complianceAlerts = missingTax
        });
    }

    public async Task<Result<IReadOnlyList<object>>> ListMerchantApplicationsAsync(string? status,
        CancellationToken cancellationToken = default)
    {
        var q = db.MerchantApplications.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MerchantApplicationStatus>(status, true, out var st))
            q = q.Where(a => a.Status == st);

        var list = await q
            .OrderByDescending(a => a.SubmittedAtUtc)
            .Take(100)
            .Select(a => new
            {
                a.Id,
                a.BusinessName,
                a.ApplicantName,
                businessType = a.BusinessType.ToString(),
                applicationStatus = a.Status.ToString(),
                documentStatus = a.DocumentStatus.ToString(),
                a.TaxId,
                a.SubmittedAtUtc
            })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }

    public async Task<Result<object>> ApproveApplicationAsync(int id, Guid? adminUserId,
        CancellationToken cancellationToken = default)
    {
        var app = await db.MerchantApplications
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (app is null)
            return Result<object>.Fail("Ariza topilmadi.", 404);

        if (app.Status == MerchantApplicationStatus.Verified)
            return Result<object>.Fail("Ariza allaqachon tasdiqlangan.", 409);
        if (app.Status == MerchantApplicationStatus.Rejected)
            return Result<object>.Fail("Rad etilgan ariza qayta tasdiqlanmaydi.", 409);

        app.Status = MerchantApplicationStatus.Verified;
        app.ProcessedAtUtc = DateTime.UtcNow;
        app.ProcessedByAdminId = adminUserId;
        app.DocumentStatus = DocumentReviewStatus.Verified;

        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = app.BusinessName,
            SystemCode = $"FM-{Random.Shared.Next(100000, 999999)}",
            BusinessType = app.BusinessType,
            IsVerified = true,
            OwnerUserId = app.ApplicantUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Merchants.Add(merchant);
        app.MerchantId = merchant.Id;

        var applicant = await userManager.FindByIdAsync(app.ApplicantUserId.ToString());
        if (applicant is not null && !await userManager.IsInRoleAsync(applicant, "Seller"))
            await userManager.AddToRoleAsync(applicant, "Seller");

        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { merchantId = merchant.Id, merchant.SystemCode });
    }

    public async Task<Result<object>> RejectApplicationAsync(int id, Guid? adminUserId, string reason,
        CancellationToken cancellationToken = default)
    {
        var app = await db.MerchantApplications.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (app is null)
            return Result<object>.Fail("Ariza topilmadi.", 404);
        if (app.Status == MerchantApplicationStatus.Verified)
            return Result<object>.Fail("Tasdiqlangan ariza rad etilmaydi.", 409);
        if (app.Status == MerchantApplicationStatus.Rejected)
            return Result<object>.Fail("Ariza allaqachon rad etilgan.", 409);
        app.Status = MerchantApplicationStatus.Rejected;
        app.ProcessedAtUtc = DateTime.UtcNow;
        app.ProcessedByAdminId = adminUserId;
        app.Notes = reason;
        await db.SaveChangesAsync(cancellationToken);
        return Result<object>.Ok(new { ok = true });
    }
}
