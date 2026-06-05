using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("api/admin")]
public class AdminController(IAdminService admin) : ControllerBase
{
    [HttpGet("merchant-applications/stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var r = await admin.GetMerchantApplicationStatsAsync(cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("merchant-applications")]
    public async Task<IActionResult> ListApplications([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var r = await admin.ListMerchantApplicationsAsync(status, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("merchant-applications/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var adminId = HttpContext.GetUserId();
        var r = await admin.ApproveApplicationAsync(id, adminId, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("merchant-applications/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectApplicationRequest body,
        CancellationToken cancellationToken)
    {
        var adminId = HttpContext.GetUserId();
        var r = await admin.RejectApplicationAsync(id, adminId, body.Reason, cancellationToken);
        return this.ToActionResult(r);
    }
}
