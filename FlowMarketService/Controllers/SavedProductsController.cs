using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/saved/products")]
public class SavedProductsController(IProfileService profile) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.ListSavedProductsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Save(int productId, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.SaveProductAsync(uid, productId, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Unsave(int productId, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await profile.UnsaveProductAsync(uid, productId, cancellationToken);
        return this.ToActionResult(r);
    }
}
