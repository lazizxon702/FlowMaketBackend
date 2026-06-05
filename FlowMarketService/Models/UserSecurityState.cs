namespace FlowMarketService.Models;

public class UserSecurityState
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public bool TwoFactorEnabled { get; set; }
    public bool Sms2FaEnabled { get; set; }
    public bool TotpEnabled { get; set; }
    public bool BiometricEnabled { get; set; }
    public DateTime? LastSecurityCheckUtc { get; set; }
}
