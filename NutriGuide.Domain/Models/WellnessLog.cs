using System.ComponentModel.DataAnnotations;


namespace NutriGuide.Domain.Models;

public class WellnessLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string Symptoms { get; set; } = string.Empty;

    public string? AiAnalysis { get; set; }
    public string? SuggestedMeal { get; set; }
}