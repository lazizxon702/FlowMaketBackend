using FlowMarket.Application.Abstractions;
using FlowMarket.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlowMarket.Domain.Entities;

namespace FlowMarket.Application.Auth;

public sealed record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AuthResponse>;

public sealed class RefreshTokenCommandHandler(
    IAppDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var stored = await dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == command.Request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (stored.IsRevoked || stored.ExpiresAtUtc <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");

        stored.IsRevoked = true;
        stored.RevokedAtUtc = DateTime.UtcNow;

        var roles = await userManager.GetRolesAsync(stored.User);
        var accessToken = jwtTokenService.GenerateAccessToken(stored.User, roles);
        var nextRefreshToken = new RefreshToken
        {
            Token = jwtTokenService.GenerateRefreshToken(),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            UserId = stored.UserId
        };
        dbContext.RefreshTokens.Add(nextRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AuthResponse(accessToken, nextRefreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }
}
