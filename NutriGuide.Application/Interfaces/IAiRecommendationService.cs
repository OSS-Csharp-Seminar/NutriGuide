using NutriGuide.Application.DTOs.AiRecommendation;

namespace NutriGuide.Application.Interfaces;

public interface IAiRecommendationService
{
    Task<AiRecommendationDto> GenerateNextMealAsync(string userId);
    Task<AiRecommendationDto> GenerateTargetMissAsync(string userId);
    Task<List<AiRecommendationDto>> GetUnreadAsync(string userId);
    Task MarkAsReadAsync(string userId, Guid id);
}