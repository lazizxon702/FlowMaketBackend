using FlowMarketService.Common;
using FlowMarketService.Contracts.Profile;

namespace FlowMarketService.Services.Interfaces;

public interface IProfileService
{
    Task<Result<object>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<object>> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> ListSavedProductsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> SaveProductAsync(Guid userId, int productId, CancellationToken cancellationToken = default);
    Task<Result<object?>> UnsaveProductAsync(Guid userId, int productId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<object>>> ListNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> MarkNotificationReadAsync(Guid userId, long id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateSupportTicketAsync(Guid userId, SupportTicketRequest request, CancellationToken cancellationToken = default);
    Task<Result<object>> GetSecurityStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ActiveSessionDto>>> ListDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> LogoutAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> RemoveDeviceAsync(Guid userId, Guid deviceId, CancellationToken cancellationToken = default);
    Task<Result<object>> GetNotificationSettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<object>> PatchNotificationSettingsAsync(Guid userId, PatchNotificationSettingsRequest request, CancellationToken cancellationToken = default);
}
