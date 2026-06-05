namespace FlowMarketService.Models;

public class UserNotificationPreferences
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public bool OrderStatusEnabled { get; set; } = true;
    public bool SecurityEnabled { get; set; } = true;
    public bool FlashSalesEnabled { get; set; }
    public bool NewArrivalsEnabled { get; set; } = true;
    public bool AiDigestComingSoon { get; set; }
}
