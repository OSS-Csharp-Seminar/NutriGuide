using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Domain.Models;


public class MessagingConnection
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public MessagingPlatform Platform { get; set; }

    [Required]
    [MaxLength(200)]
    public string ExternalUserId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MessagingLog> MessagingLogs { get; set; } = new List<MessagingLog>();
}