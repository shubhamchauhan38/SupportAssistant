
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportAssistant.API.Data;
using SupportAssistant.API.DTOs;
using SupportAssistant.API.Models;
using SupportAssistant.API.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SupportAssistant.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;

    public TicketsController(ApplicationDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    // GET: api/tickets
    [HttpGet]
    public async Task<IActionResult> GetTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (userRole == "SupportEngineer")
        {
            var allTickets = await _context.Tickets
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(allTickets);
        }

        var employeeTickets = await _context.Tickets
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(employeeTickets);
    }

    // GET: api/tickets/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    // POST: api/tickets
    [HttpPost]
    public async Task<ActionResult<Ticket>> CreateTicket(
        CreateTicketRequest request)
    {
        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Environment = request.Environment,
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
        };

        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTicket),
            new { id = ticket.Id },
            ticket);
    }

    [HttpPost("{id:int}/analyze")]
    public async Task<ActionResult<TicketAnalysisResponse>> AnalyzeTicket(
    int id,
    CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets.FindAsync(
            new object[] { id }, cancellationToken);

        if (ticket == null)
        {
            return NotFound();
        }

        // Send the ticket to Gemini
        var analysis = await _aiService.AnalyzeTicketAsync(
            ticket, cancellationToken);

        // Save the AI response in SQLite
        analysis.ApprovalStatus = "Pending";

        ticket.AIAnalysisJson = JsonSerializer.Serialize(analysis);
        ticket.AIAnalyzedAt = DateTime.UtcNow;
        ticket.IsAnalysisApproved = false;
        ticket.AIAnalysisStatus = "Pending";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(analysis);
    }


    [HttpGet("{id:int}/analysis")]
    public async Task<ActionResult<TicketAnalysisResponse>> GetTicketAnalysis(
        int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(ticket.AIAnalysisJson))
        {
            return NotFound("No AI analysis exists for this ticket.");
        }

        var analysis =
            JsonSerializer.Deserialize<TicketAnalysisResponse>(
                ticket.AIAnalysisJson);

        if (analysis == null)
        {
            return Problem("Saved AI analysis could not be read.");
        }

        return Ok(analysis);
    }
    [Authorize(Roles = "SupportEngineer")]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<TicketAnalysisResponse>> ApproveAnalysis(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return NotFound("Ticket not found.");
        }

        if (string.IsNullOrWhiteSpace(ticket.AIAnalysisJson))
        {
            return BadRequest("No AI analysis exists for this ticket.");
        }

        var analysis = JsonSerializer.Deserialize<TicketAnalysisResponse>(
            ticket.AIAnalysisJson);

        if (analysis == null)
        {
            return Problem("Saved AI analysis could not be read.");
        }

        analysis.ApprovalStatus = "Approved";

        ticket.AIAnalysisStatus = "Approved";
        ticket.IsAnalysisApproved = true;
        ticket.AIAnalysisJson = JsonSerializer.Serialize(analysis);

        var review = new TicketReview
        {
            TicketId = ticket.Id,
            Action = "Approved",
            ReviewedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            ReviewedByEmail = User.FindFirstValue(ClaimTypes.Email)!,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketReviews.Add(review);

        await _context.SaveChangesAsync();

        return Ok(analysis);
    }

    [Authorize(Roles = "SupportEngineer")]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<TicketAnalysisResponse>> RejectAnalysis(
    int id,
    [FromBody] string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest("A rejection reason is required.");
        }

        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return NotFound("Ticket not found.");
        }

        if (string.IsNullOrWhiteSpace(ticket.AIAnalysisJson))
        {
            return BadRequest("No AI analysis exists for this ticket.");
        }

        var analysis = JsonSerializer.Deserialize<TicketAnalysisResponse>(
            ticket.AIAnalysisJson);

        if (analysis == null)
        {
            return Problem("Saved AI analysis could not be read.");
        }

        analysis.ApprovalStatus = "Rejected";

        ticket.AIAnalysisStatus = "Rejected";
        ticket.IsAnalysisApproved = false;
        ticket.AIAnalysisJson = JsonSerializer.Serialize(analysis);

        var review = new TicketReview
        {
            TicketId = ticket.Id,
            Action = "Rejected",
            ReviewedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            ReviewedByEmail = User.FindFirstValue(ClaimTypes.Email)!,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketReviews.Add(review);

        await _context.SaveChangesAsync();

        return Ok(analysis);
    }
}
