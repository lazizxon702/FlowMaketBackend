using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlowMarketService.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register/email")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterByEmail([FromBody] RegisterByEmailRequest body, CancellationToken cancellationToken)
    {
        var request = new RegisterRequest
        {
            FullName = body.FullName,
            Mode = "email",
            AcceptTerms = body.AcceptTerms,
            Email = body.Email,
            Password = body.Password,
            ConfirmPassword = body.ConfirmPassword,
            Handle = body.Handle,
            ReferralCode = body.ReferralCode
        };
        var r = await auth.RegisterAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("register/phone")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterByPhone([FromBody] RegisterByPhoneRequest body, CancellationToken cancellationToken)
    {
        var request = new RegisterRequest
        {
            FullName = body.FullName,
            Mode = "phone",
            AcceptTerms = body.AcceptTerms,
            Phone = body.Phone,
            Password = body.Password,
            ConfirmPassword = body.ConfirmPassword,
            Handle = body.Handle,
            ReferralCode = body.ReferralCode
        };
        var r = await auth.RegisterAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    /// <summary>Umumiy ro‘yxatdan o‘tish (mode: email | phone). Ikkita alohida endpoint ham mavjud.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var r = await auth.RegisterAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var r = await auth.LoginAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("login/email")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginByEmail([FromBody] LoginByEmailRequest body, CancellationToken cancellationToken)
    {
        var request = new LoginRequest
        {
            Email = body.Email,
            Password = body.Password
        };
        var r = await auth.LoginAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("login/phone")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginByPhone([FromBody] LoginByPhoneRequest body, CancellationToken cancellationToken)
    {
        var request = new LoginRequest
        {
            Email = body.Phone,
            Password = body.Password
        };
        var r = await auth.LoginAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var r = await auth.RefreshAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await auth.ChangePasswordAsync(uid, request, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("admin/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var r = await auth.AdminResetPasswordAsync(request, cancellationToken);
        return this.ToActionResult(r);
    }
}
