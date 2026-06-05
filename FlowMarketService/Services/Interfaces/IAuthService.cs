using FlowMarketService.Common;
using FlowMarketService.Contracts;

namespace FlowMarketService.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<object>> AdminResetPasswordAsync(AdminResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}