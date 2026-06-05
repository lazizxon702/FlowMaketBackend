namespace FlowMarketService.Contracts.Profile;

public record UpdateProfileRequest(
    string? FullName,
    string? Handle,
    string? Location,
    string? ProfilePictureUrl,
    DateOnly? DateOfBirth,
    string? Phone,
    string? AccountType);

public record SupportTicketRequest(string Subject, string Message);

public record PatchNotificationSettingsRequest(
    bool? OrderStatusEnabled,
    bool? SecurityEnabled,
    bool? FlashSalesEnabled,
    bool? NewArrivalsEnabled);

public sealed record ActiveSessionDto(
    Guid Id,
    string Title,
    string Subtitle,
    string LastActivityText,
    DateTime LastActiveUtc,
    bool IsCurrent);
