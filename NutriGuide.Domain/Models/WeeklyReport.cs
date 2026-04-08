using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NutriGuide.Domain.Models;

public class WeeklyReport
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateOnly WeekStart { get; set; }

    [Required]
    public DateOnly WeekEnd { get; set; }

    public decimal? AvgCalories { get; set; }
    public decimal? AvgProtein_g { get; set; }
    public decimal? AvgCarbs_g { get; set; }
    public decimal? AvgFat_g { get; set; }
    public decimal? GoalAdherence_pct { get; set; }
    public string? AiSummary { get; set; }

    public Guid? WeightProgressId { get; set; }

    [ForeignKey("WeightProgressId")]
    public WeightProgress? WeightProgress { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}