namespace FlowMarketService.Models;

public class ShoppingCart
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? AppliedPromoCode { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
