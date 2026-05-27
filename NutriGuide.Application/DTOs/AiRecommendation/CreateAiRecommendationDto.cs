using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.AiRecommendation;

public class CreateAiRecommendationDto
{
    public TriggerType TriggerType { get; set; }
}