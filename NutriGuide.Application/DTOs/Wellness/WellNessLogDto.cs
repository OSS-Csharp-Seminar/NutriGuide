namespace NutriGuide.Application.DTOs.Wellness;

public class WellnessLogDto
{
    public Guid Id { get; set; }
    public DateTime LoggedAt { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string? AiAnalysis { get; set; }
    public string? SuggestedMeal { get; set; }
}