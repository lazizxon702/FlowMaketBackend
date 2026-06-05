using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] int? categoryId, [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var r = await catalog.GetProductsAsync(categoryId, includeInactive, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var r = await catalog.GetProductAsync(id, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest body, CancellationToken cancellationToken)
    {
        var r = await catalog.CreateProductAsync(body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest body,
        CancellationToken cancellationToken)
    {
        var r = await catalog.UpdateProductAsync(id, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var r = await catalog.DeleteProductAsync(id, cancellationToken);
        return this.ToActionResult(r);
    }
}
