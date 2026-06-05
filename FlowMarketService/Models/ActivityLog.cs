namespace FlowMarketService.Models;

public class ActivityLog
{
    public long Id { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;

    public ActivityType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MetaJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
