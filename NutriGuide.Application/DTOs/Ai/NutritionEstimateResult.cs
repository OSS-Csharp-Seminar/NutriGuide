namespace NutriGuide.Application.DTOs.Ai;

public class NutritionEstimateResult
{
    public int Calories { get; set; }
    public decimal Protein_g { get; set; }
    public decimal Carbs_g { get; set; }
    public decimal Fat_g { get; set; }
    public decimal Fiber_g { get; set; }
    public string AiNote { get; set; } = string.Empty;
}