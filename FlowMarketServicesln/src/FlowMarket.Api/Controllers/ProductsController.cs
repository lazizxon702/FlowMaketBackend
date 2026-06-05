using FlowMarket.Application.Contracts;
using FlowMarket.Application.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowMarket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> Get(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetProductsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new CreateProductCommand(request), cancellationToken));
}
