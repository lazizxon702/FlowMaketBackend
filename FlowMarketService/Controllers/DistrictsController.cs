using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/districts")]
public class DistrictsController(ICommerceService commerce) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDistricts([FromQuery] string? city, CancellationToken cancellationToken)
    {
        var r = await commerce.GetDistrictsAsync(city, cancellationToken);
        return this.ToActionResult(r);
    }
}
