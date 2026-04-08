using System.ComponentModel.DataAnnotations;

namespace NutriGuide.Domain.Models;

public class DailyNutritionSummary
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateOnly SummaryDate { get; set; }

    public int TotalCalories { get; set; } = 0;
    public decimal TotalProtein_g { get; set; } = 0;
    public decimal TotalCarbs_g { get; set; } = 0;
    public decimal TotalFat_g { get; set; } = 0;
    public decimal TotalFiber_g { get; set; } = 0;
    public int MealCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}