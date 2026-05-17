using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.MealLog;

public class MealLogDto
{
    public Guid Id { get; set; }
    public string RawInput { get; set; } = string.Empty;
    public MealType? MealType { get; set; }
    public DateTime LoggedAt { get; set; }
    public int? Calories { get; set; }
    public decimal? Protein_g { get; set; }
    public decimal? Carbs_g { get; set; }
    public decimal? Fat_g { get; set; }
    public decimal? Fiber_g { get; set; }
    public string? AiNote { get; set; }
    public MealSource Source { get; set; }
}