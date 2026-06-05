using FlowMarket.Application.Abstractions;
using FlowMarket.Application.Contracts;
using FlowMarket.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlowMarket.Application.Auth;

public sealed record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    IAppDbContext dbContext) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Email == command.Request.Email, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");
        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, command.Request.Password, true);
        if (!passwordResult.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refresh = new RefreshToken
        {
            Token = jwtTokenService.GenerateRefreshToken(),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            UserId = user.Id
        };
        dbContext.RefreshTokens.Add(refresh);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AuthResponse(accessToken, refresh.Token, DateTime.UtcNow.AddMinutes(15));
    }
}
