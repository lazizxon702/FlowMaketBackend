namespace FlowMarketService.Models;

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SystemCode { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public bool IsVerified { get; set; }
    public Guid OwnerUserId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<MerchantApplication> Applications { get; set; } = new List<MerchantApplication>();
    public ICollection<MerchantContract> Contracts { get; set; } = new List<MerchantContract>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
