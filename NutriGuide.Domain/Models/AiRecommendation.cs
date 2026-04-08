using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Domain.Models;


public class AiRecommendation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    [Required]
    public TriggerType TriggerType { get; set; }

    [Required]
    public string RecommendationText { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
}