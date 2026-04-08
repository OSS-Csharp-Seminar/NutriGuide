using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriGuide.Domain.Models;

public class MessagingLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ConnectionId { get; set; }

    [ForeignKey("ConnectionId")]
    public MessagingConnection Connection { get; set; } = null!;

    [Required]
    public string RawMessage { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public Guid? MealLogId { get; set; }

    [ForeignKey("MealLogId")]
    public MealLog? MealLog { get; set; }

    public bool IsProcessed { get; set; } = false;
    public string? ProcessingError { get; set; }
}