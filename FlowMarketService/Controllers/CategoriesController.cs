using FlowMarketService.Contracts;
using FlowMarketService.Infrastructure;
using FlowMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarketService.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var r = await catalog.GetCategoriesAsync(cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var r = await catalog.GetCategoryAsync(id, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest body, CancellationToken cancellationToken)
    {
        var r = await catalog.CreateCategoryAsync(body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest body,
        CancellationToken cancellationToken)
    {
        var r = await catalog.UpdateCategoryAsync(id, body, cancellationToken);
        return this.ToActionResult(r);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var r = await catalog.DeleteCategoryAsync(id, cancellationToken);
        return this.ToActionResult(r);
    }
}
