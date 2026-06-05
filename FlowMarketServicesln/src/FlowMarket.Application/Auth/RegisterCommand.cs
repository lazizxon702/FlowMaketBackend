using FlowMarket.Application.Abstractions;
using FlowMarket.Application.Contracts;
using FlowMarket.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FlowMarket.Application.Auth;

public sealed record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

public sealed class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IAppDbContext dbContext) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = command.Request.Email,
            Email = command.Request.Email,
            FullName = command.Request.FullName
        };

        var createResult = await userManager.CreateAsync(user, command.Request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
            throw new InvalidOperationException(errors);
        }

        await userManager.AddToRoleAsync(user, "User");
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
