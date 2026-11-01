
namespace SupportAssistant.API.Models;

public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? AIAnalysisJson { get; set; }

    public DateTime? AIAnalyzedAt { get; set; }

    public bool IsAnalysisApproved { get; set; } = false;
    public string AIAnalysisStatus { get; set; } = "Pending";
    public string? CreatedByUserId { get; set; }
}
