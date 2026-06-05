using FlowMarketService.Common;

namespace FlowMarketService.Services.Interfaces;

public interface ILegalDocumentService
{
    Task<Result<IReadOnlyList<object>>> ListAsync(CancellationToken cancellationToken = default);
}
