namespace SupportAssistant.API.Models;

public class TicketReview
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string ReviewedByUserId { get; set; } = string.Empty;

    public string ReviewedByEmail { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
}