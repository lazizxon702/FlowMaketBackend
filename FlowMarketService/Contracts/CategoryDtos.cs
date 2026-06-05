namespace FlowMarketService.Contracts;

public record CategoryResponse(int Id, string Name, string? Description);

public record CreateCategoryRequest(string Name, string? Description);

public record UpdateCategoryRequest(string Name, string? Description);