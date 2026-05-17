using NutriGuide.Application.DTOs.MealLog;

namespace NutriGuide.Application.DTOs.MealLog;

public class DailyMealSummaryDto
{
    public DateOnly Date { get; set; }
    public List<MealLogDto> Meals { get; set; } = new();
    public int TotalCalories { get; set; }
    public decimal TotalProtein_g { get; set; }
    public decimal TotalCarbs_g { get; set; }
    public decimal TotalFat_g { get; set; }
    public decimal TotalFiber_g { get; set; }
    public int MealCount { get; set; }
}