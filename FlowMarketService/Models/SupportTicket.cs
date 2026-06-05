namespace FlowMarketService.Models;

public class SupportTicket
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
    public DateTime CreatedAtUtc { get; set; }
}
