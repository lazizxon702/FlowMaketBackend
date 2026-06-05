namespace FlowMarketService.Models;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid? MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AttributesSummary { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTrending { get; set; }
    public int SalesThisMonth { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<SavedProduct> SavedByUsers { get; set; } = new List<SavedProduct>();
}