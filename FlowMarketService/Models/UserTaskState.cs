namespace FlowMarketService.Models;

public class UserTaskState
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public EarnTaskType TaskType { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? LastDailyClaimUtc { get; set; }
}
