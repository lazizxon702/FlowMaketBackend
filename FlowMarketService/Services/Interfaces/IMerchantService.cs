using FlowMarketService.Common;
using FlowMarketService.Contracts;

namespace FlowMarketService.Services.Interfaces;

public interface IMerchantService
{
    Task<Result<int>> SubmitApplicationAsync(Guid userId, MerchantApplyRequest request, CancellationToken cancellationToken = default);
    Task<Result<object>> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> GetContractsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> GetActivityAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> GetTopProductsAsync(Guid userId, CancellationToken cancellationToken = default);
}
