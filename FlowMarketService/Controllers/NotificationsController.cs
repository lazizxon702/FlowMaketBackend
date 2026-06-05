using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(IProfileService profile) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.ListNotificationsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPatch("{id:long}/read")]
    public async Task<IActionResult> MarkRead(long id, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.MarkNotificationReadAsync(uid, id, cancellationToken);
        return this.ToActionResult(r);
    }
}
