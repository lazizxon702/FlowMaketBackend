using FlowMarketService.Contracts.Profile;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/support")]
public class SupportController(IProfileService profile) : ControllerBase
{
    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] SupportTicketRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.CreateSupportTicketAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }
}
