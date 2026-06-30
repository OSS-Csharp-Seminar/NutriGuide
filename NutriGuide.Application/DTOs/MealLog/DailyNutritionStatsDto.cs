namespace NutriGuide.Application.DTOs.MealLog;

public class DailyNutritionStatsDto
{
    public DateOnly Date { get; set; }
    public int Calories { get; set; }
    public decimal Protein_g { get; set; }
    public decimal Carbs_g { get; set; }
    public decimal Fat_g { get; set; }
    public decimal Fiber_g { get; set; }
    public int MealCount { get; set; }

    public int TargetCalories { get; set; }
    public decimal TargetProtein_g { get; set; }
    public decimal TargetCarbs_g { get; set; }
    public decimal TargetFat_g { get; set; }
    public decimal TargetFiber_g { get; set; }
}
