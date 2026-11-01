using SupportAssistant.API.DTOs;
using SupportAssistant.API.Models;

namespace SupportAssistant.API.Services;

public interface IAIService
{
    Task<TicketAnalysisResponse> AnalyzeTicketAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);
}