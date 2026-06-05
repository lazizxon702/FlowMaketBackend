namespace FlowMarketService.Models;

public class Wallet
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public decimal CoinBalance { get; set; }
    public decimal CreditUzs { get; set; }
}
