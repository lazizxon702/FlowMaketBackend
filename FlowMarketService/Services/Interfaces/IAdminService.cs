using FlowMarketService.Common;

namespace FlowMarketService.Services.Interfaces;

public interface IAdminService
{
    Task<Result<object>> GetMerchantApplicationStatsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> ListMerchantApplicationsAsync(string? status, CancellationToken cancellationToken = default);
    Task<Result<object>> ApproveApplicationAsync(int id, Guid? adminUserId, CancellationToken cancellationToken = default);
    Task<Result<object>> RejectApplicationAsync(int id, Guid? adminUserId, string reason, CancellationToken cancellationToken = default);
}
