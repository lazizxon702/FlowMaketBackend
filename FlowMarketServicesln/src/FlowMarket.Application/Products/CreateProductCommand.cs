using AutoMapper;
using FlowMarket.Application.Abstractions;
using FlowMarket.Application.Contracts;
using FlowMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowMarket.Application.Products;

public sealed record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductDto>;
public sealed record GetProductsQuery() : IRequest<IReadOnlyCollection<ProductDto>>;

public sealed class CreateProductCommandHandler(IAppDbContext dbContext, IMapper mapper)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Request.Name,
            Description = command.Request.Description,
            Price = command.Request.Price,
            StockQuantity = command.Request.StockQuantity
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<ProductDto>(product);
    }
}

public sealed class GetProductsQueryHandler(IAppDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var data = await dbContext.Products.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return mapper.Map<IReadOnlyCollection<ProductDto>>(data);
    }
}
