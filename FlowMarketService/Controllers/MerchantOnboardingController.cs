using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/merchant-onboarding")]
public class MerchantOnboardingController(IMerchantService merchant) : ControllerBase
{
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] MerchantApplyRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await merchant.SubmitApplicationAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }
}
