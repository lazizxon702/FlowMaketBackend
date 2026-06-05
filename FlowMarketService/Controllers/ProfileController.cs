using FlowMarketService.Contracts.Profile;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController(IProfileService profile) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.GetProfileAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.UpdateProfileAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.DeleteAccountAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.GetSummaryAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }
}
