using FlowMarketService.Contracts.Commerce;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public class PaymentsController(ICommerceService commerce) : ControllerBase
{
    [HttpGet("cards")]
    public async Task<IActionResult> ListCards(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.ListCardsAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost("cards")]
    public async Task<IActionResult> AddCard([FromBody] AddCardRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.AddCardAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("cards/{id:guid}")]
    public async Task<IActionResult> DeleteCard(Guid id, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.DeleteCardAsync(uid, id, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPatch("cards/{id:guid}/primary")]
    public async Task<IActionResult> SetPrimaryCard(Guid id, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.SetPrimaryCardAsync(uid, id, cancellationToken);
        return this.ToActionResult(r);
    }
}
