using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.AiRecommendation;

public class AiRecommendationDto
{
    public Guid Id { get; set; }
    public TriggerType TriggerType { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRead { get; set; }
}