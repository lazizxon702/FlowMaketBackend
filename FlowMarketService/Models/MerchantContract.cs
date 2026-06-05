namespace FlowMarketService.Models;

public class MerchantContract
{
    public int Id { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string Category { get; set; } = "Service";
    public ContractStatus Status { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string? PdfUrl { get; set; }
    public string? ContentSummary { get; set; }
    public DateTime? SignedAtUtc { get; set; }
    public string? SignatoryName { get; set; }
    public string? DigitalFingerprint { get; set; }
}
