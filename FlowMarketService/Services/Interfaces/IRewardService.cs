using FlowMarketService.Common;

namespace FlowMarketService.Services.Interfaces;

public interface IRewardService
{
    Task<Result<object>> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> LuckySpinAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> ConvertAsync(Guid userId, decimal coins, CancellationToken cancellationToken = default);
    Task<Result<object>> DailyCheckInAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> CompleteReviewAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> CompleteKycAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> GetTaskStatesAsync(Guid userId, CancellationToken cancellationToken = default);
}
