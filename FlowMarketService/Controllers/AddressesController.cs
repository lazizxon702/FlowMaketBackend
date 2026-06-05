using FlowMarketService.Contracts.Commerce;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Authorize]
[Route("api/addresses")]
public class AddressesController(ICommerceService commerce) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.ListAddressesAsync(uid, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddressRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.CreateAddressAsync(uid, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AddressRequest body, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.UpdateAddressAsync(uid, id, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.DeleteAddressAsync(uid, id, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPatch("{id:int}/primary")]
    public async Task<IActionResult> SetPrimary(int id, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId()!.Value;
        var r = await commerce.SetPrimaryAddressAsync(uid, id, cancellationToken);
        return this.ToActionResult(r);
    }
}
