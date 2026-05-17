using NutriGuide.Application.DTOs.Ai;

namespace NutriGuide.Application.Interfaces;

public interface IAiService
{
    Task<NutritionEstimateResult> EstimateNutritionAsync(string mealDescription);
}