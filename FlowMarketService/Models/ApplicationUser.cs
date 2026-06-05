using Microsoft.AspNetCore.Identity;

namespace FlowMarketService.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string? Handle { get; set; }
    public string? Location { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime? LastPasswordChangedUtc { get; set; }
    public string AccountType { get; set; } = "Personal";
    public string? ReferralCodeOwned { get; set; }
    public Guid? ReferredByUserId { get; set; }
    public ApplicationUser? ReferredByUser { get; set; }
    public bool IdentityVerified { get; set; }

    public Wallet? Wallet { get; set; }
    public UserNotificationPreferences? NotificationPreferences { get; set; }
    public UserSecurityState? SecurityState { get; set; }
    public ICollection<Merchant> OwnedMerchants { get; set; } = new List<Merchant>();
    public ICollection<MerchantApplication> MerchantApplications { get; set; } = new List<MerchantApplication>();
}
