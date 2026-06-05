namespace FlowMarketService.Models;

public class UserDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? LocationLabel { get; set; }
    public DateTime LastActiveUtc { get; set; }
    public bool IsCurrent { get; set; }
}
