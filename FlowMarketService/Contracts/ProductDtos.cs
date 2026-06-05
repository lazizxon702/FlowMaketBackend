namespace FlowMarketService.Contracts;

public record ProductResponse(
    int Id,
    int CategoryId,
    string CategoryName,
    Guid? MerchantId,
    string? MerchantName,
    string Name,
    string? Description,
    string? AttributesSummary,
    decimal Price,
    int Stock,
    string? ImageUrl,
    bool IsActive,
    bool IsTrending,
    DateTime CreatedAtUtc);

public record CreateProductRequest(
    int CategoryId,
    Guid? MerchantId,
    string Name,
    string? Description,
    string? AttributesSummary,
    decimal Price,
    int Stock,
    string? ImageUrl,
    bool IsActive,
    bool IsTrending);

public record UpdateProductRequest(
    string Name,
    string? Description,
    string? AttributesSummary,
    decimal Price,
    int Stock,
    string? ImageUrl,
    bool IsActive,
    bool IsTrending);