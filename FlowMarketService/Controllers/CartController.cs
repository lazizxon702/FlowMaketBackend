using FlowMarketService.Contracts.Commerce;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController(ICommerceService commerce) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.GetCartAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.AddCartItemAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPatch("items/{itemId:int}")]
    public async Task<IActionResult> PatchItem(int itemId, [FromBody] PatchCartItemRequest body,
        CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.PatchCartItemAsync(uid, itemId, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int itemId, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.RemoveCartItemAsync(uid, itemId, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("promo")]
    public async Task<IActionResult> ApplyPromo([FromBody] ApplyPromoRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.ApplyPromoAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }
}
