using FlowMarketService.Contracts.Commerce;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/checkout")]
public class CheckoutController(ICommerceService commerce) : ControllerBase
{
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] CheckoutRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.ConfirmCheckoutAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }
}
