using FlowMarketService.Contracts.Profile;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController(IProfileService profile) : ControllerBase
{
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotificationSettings(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.GetNotificationSettingsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPatch("notifications")]
    public async Task<IActionResult> PatchNotificationSettings([FromBody] PatchNotificationSettingsRequest body,
        CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.PatchNotificationSettingsAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }
}
