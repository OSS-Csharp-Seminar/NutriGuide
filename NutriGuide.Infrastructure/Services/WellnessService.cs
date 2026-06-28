using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.Wellness;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class WellnessService : IWellnessService
{
    private readonly AppDbContext _context;
    private readonly IAiService _aiService;

    public WellnessService(AppDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<WellnessLogDto> CreateAsync(string userId, CreateWellnessLogDto dto)
    {
        
        var cutoff = DateTime.UtcNow.AddHours(-48);
        var recentMeals = await _context.MealLogs
            .Where(ml => ml.UserId == userId && ml.LoggedAt >= cutoff)
            .OrderByDescending(ml => ml.LoggedAt)
            .ToListAsync();

       
        var analysis = await _aiService.AnalyzeWellnessAsync(dto.Symptoms, recentMeals);

        var wellnessLog = new WellnessLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LoggedAt = DateTime.UtcNow,
            Symptoms = dto.Symptoms,
            AiAnalysis = analysis.Analysis,
            SuggestedMeal = analysis.SuggestedMeal
        };

        _context.WellnessLogs.Add(wellnessLog);

        
        var recommendation = new AiRecommendation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TriggerType = Domain.Enums.TriggerType.Wellness,
            RecommendationText = $"{analysis.Analysis}\n\nPrijedlog obroka: {analysis.SuggestedMeal}",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsRead = false
        };

        _context.AiRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();

        return MapToDto(wellnessLog);
    }

    public async Task<List<WellnessLogDto>> GetHistoryAsync(string userId)
    {
        return await _context.WellnessLogs
            .Where(wl => wl.UserId == userId)
            .OrderByDescending(wl => wl.LoggedAt)
            .Select(wl => MapToDto(wl))
            .ToListAsync();
    }

    private static WellnessLogDto MapToDto(WellnessLog wl) => new()
    {
        Id = wl.Id,
        LoggedAt = wl.LoggedAt,
        Symptoms = wl.Symptoms,
        AiAnalysis = wl.AiAnalysis,
        SuggestedMeal = wl.SuggestedMeal
    };
}