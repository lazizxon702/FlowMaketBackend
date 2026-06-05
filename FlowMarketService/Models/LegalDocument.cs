namespace FlowMarketService.Models;

public class LegalDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime PublishedAtUtc { get; set; }
}
