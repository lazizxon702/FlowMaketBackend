namespace FlowMarket.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
}
