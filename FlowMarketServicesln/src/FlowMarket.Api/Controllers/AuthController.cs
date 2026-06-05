using FlowMarket.Application.Auth;
using FlowMarket.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new RegisterCommand(request), cancellationToken));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new LoginCommand(request), cancellationToken));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new RefreshTokenCommand(request), cancellationToken));
}
