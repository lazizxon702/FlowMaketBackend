using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize(Policy = "SellerOrAdmin")]
[Route("api/merchant")]
public class MerchantController(IMerchantService merchant) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await merchant.GetDashboardAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("contracts")]
    public async Task<IActionResult> Contracts(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await merchant.GetContractsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("activity")]
    public async Task<IActionResult> Activity(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await merchant.GetActivityAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("products/top")]
    public async Task<IActionResult> TopProducts(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await merchant.GetTopProductsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }
}
