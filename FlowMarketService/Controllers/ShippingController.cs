using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/shipping")]
public class ShippingController(ICommerceService commerce) : ControllerBase
{
    [HttpGet("options")]
    public async Task<IActionResult> GetShippingOptions(CancellationToken cancellationToken)
    {
        var r = await commerce.GetShippingOptionsAsync(cancellationToken);
        return this.ToActionResult(r);
    }
}
