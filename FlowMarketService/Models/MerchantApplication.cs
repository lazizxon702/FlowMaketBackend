namespace FlowMarketService.Models;

public class MerchantApplication
{
    public int Id { get; set; }
    public Guid? MerchantId { get; set; }
    public Merchant? Merchant { get; set; }

    public string ApplicantName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public Guid ApplicantUserId { get; set; }
    public ApplicationUser Applicant { get; set; } = null!;

    public MerchantApplicationStatus Status { get; set; } = MerchantApplicationStatus.Pending;
    public DocumentReviewStatus DocumentStatus { get; set; } = DocumentReviewStatus.Pending;
    public string? TaxId { get; set; }
    public string? Notes { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public Guid? ProcessedByAdminId { get; set; }
}
