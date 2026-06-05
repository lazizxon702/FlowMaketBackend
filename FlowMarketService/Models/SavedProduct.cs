namespace FlowMarketService.Models;

public class SavedProduct
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public DateTime SavedAtUtc { get; set; }
}
