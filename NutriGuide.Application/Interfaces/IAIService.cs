using NutriGuide.Application.DTOs.Ai;
using NutriGuide.Domain.Models;

namespace NutriGuide.Application.Interfaces;

public interface IAiService
{
    Task<NutritionEstimateResult> EstimateNutritionAsync(string mealDescription);
    Task<string> GenerateNextMealRecommendationAsync(List<MealLog> todaysMeals, DailyTarget target);
    Task<string> GenerateTargetMissRecommendationAsync(List<MealLog> todaysMeals, DailyTarget target);
    Task<WellnessAnalysisResult> AnalyzeWellnessAsync(string symptoms, List<MealLog> last48HoursMeals);
}