
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportAssistant.API.Data;
using SupportAssistant.API.Models;

namespace SupportAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReviewController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{ticketId:int}/reviews")]
    public async Task<ActionResult<List<TicketReview>>> GetTicketReviews(
        int ticketId)
    {
        // Check whether the ticket exists.
        var ticketExists = await _context.Tickets
            .AnyAsync(t => t.Id == ticketId);

        if (!ticketExists)
        {
            return NotFound("Ticket not found.");
        }

        // Get all reviews belonging to this ticket.
        var reviews = await _context.TicketReviews
            .Where(r => r.TicketId == ticketId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reviews);
    }
}
