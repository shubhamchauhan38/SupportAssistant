
using System.ComponentModel.DataAnnotations;

namespace SupportAssistant.API.DTOs;

public class CreateTicketRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Environment { get; set; } = string.Empty;
}
