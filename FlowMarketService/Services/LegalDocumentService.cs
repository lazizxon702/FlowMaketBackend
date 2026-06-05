using FlowMarketService.Common;
using FlowMarketService.Data;
using FlowMarketService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowMarketService.Services;

public class LegalDocumentService(AppDbContext db) : ILegalDocumentService
{
    public async Task<Result<IReadOnlyList<object>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = await db.LegalDocuments.AsNoTracking()
            .OrderBy(d => d.Title)
            .Select(d => new { d.Id, d.Title, d.Version, d.FileUrl, d.PublishedAtUtc })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<object>>.Ok(list);
    }
}
