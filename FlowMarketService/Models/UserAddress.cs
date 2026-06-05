namespace FlowMarketService.Models;

public class UserAddress
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Label { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public District District { get; set; } = null!;

    public string Street { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
    public string? Apartment { get; set; }
    public string? Comment { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsPrimary { get; set; }
}
