namespace FlowMarketService.Models;

public class CoinTransaction
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public decimal Amount { get; set; }
    public CoinTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
