namespace FlowMarketService.Models;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
}
