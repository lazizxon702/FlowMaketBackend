using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/security")]
public class SecurityController(IProfileService profile) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.GetSecurityStatusAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("devices")]
    public async Task<IActionResult> ListDevices(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.ListDevicesAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.LogoutAllSessionsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("devices/{deviceId:guid}")]
    public async Task<IActionResult> RemoveDevice([FromRoute] Guid deviceId, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.RemoveDeviceAsync(uid, deviceId, cancellationToken);
        return this.ToActionResult(r);
    }
}
