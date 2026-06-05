using FlowMarketService.Common;
using FlowMarketService.Contracts;
using FlowMarketService.Models;

namespace FlowMarketService.Services.Interfaces;

public interface ICatalogService
{
    Task<Result<IReadOnlyList<CategoryResponse>>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Result<CategoryResponse>> GetCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponse>> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<object?>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ProductResponse>>> GetProductsAsync(int? categoryId, bool includeInactive, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<object?>> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<OrderResponse>>> GetOrdersAsync(Guid? currentUserId, bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<Result<OrderResponse>> GetOrderAsync(int id, Guid? currentUserId, bool isAdmin,
        CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> UpdateOrderStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default);
}
