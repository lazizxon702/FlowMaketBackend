namespace FlowMarketService.Models;

public class SavedPaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string MaskedPan { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardholderName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? PaymentToken { get; set; }
}
