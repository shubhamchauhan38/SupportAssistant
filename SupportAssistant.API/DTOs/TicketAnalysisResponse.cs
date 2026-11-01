
namespace SupportAssistant.API.DTOs;

public class TicketAnalysisResponse
{
    public string Category { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<string> SuggestedActions { get; set; } = new();

    public string DraftResponse { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = "Pending";
}
