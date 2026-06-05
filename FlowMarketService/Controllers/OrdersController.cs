using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var r = await catalog.GetOrdersAsync(uid, isAdmin, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var uid = HttpContext.GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var r = await catalog.GetOrderAsync(id, uid, isAdmin, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest body, CancellationToken cancellationToken)
    {
        var r = await catalog.CreateOrderAsync(body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest body,
        CancellationToken cancellationToken)
    {
        var r = await catalog.UpdateOrderStatusAsync(id, body.Status, cancellationToken);
        return this.ToActionResult(r);
    }
}
