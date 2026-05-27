using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.AiRecommendation;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly AppDbContext _context;
    private readonly IAiService _aiService;

    public AiRecommendationService(AppDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<AiRecommendationDto> GenerateNextMealAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todaysMeals = await _context.MealLogs
            .Where(ml => ml.UserId == userId &&
                         DateOnly.FromDateTime(ml.LoggedAt) == today)
            .ToListAsync();

        var target = await _context.DailyTargets
            .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.TargetDate == today);

        if (target == null)
            throw new KeyNotFoundException("Daily target not found.");

        var text = await _aiService.GenerateNextMealRecommendationAsync(todaysMeals, target);

        return await SaveRecommendationAsync(userId, TriggerType.NextMeal, text,
            expiresAt: DateTime.UtcNow.AddHours(6));
    }

    public async Task<AiRecommendationDto> GenerateTargetMissAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todaysMeals = await _context.MealLogs
            .Where(ml => ml.UserId == userId &&
                         DateOnly.FromDateTime(ml.LoggedAt) == today)
            .ToListAsync();

        var target = await _context.DailyTargets
            .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.TargetDate == today);

        if (target == null)
            throw new KeyNotFoundException("Daily target not found.");

        var text = await _aiService.GenerateTargetMissRecommendationAsync(todaysMeals, target);

        return await SaveRecommendationAsync(userId, TriggerType.TargetMiss, text,
            expiresAt: DateTime.UtcNow.AddHours(12));
    }

    public async Task<List<AiRecommendationDto>> GetUnreadAsync(string userId)
    {
        var now = DateTime.UtcNow;

        return await _context.AiRecommendations
            .Where(ar => ar.UserId == userId &&
                         !ar.IsRead &&
                         (ar.ExpiresAt == null || ar.ExpiresAt > now))
            .OrderByDescending(ar => ar.CreatedAt)
            .Select(ar => MapToDto(ar))
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(string userId, Guid id)
    {
        var recommendation = await _context.AiRecommendations
            .FirstOrDefaultAsync(ar => ar.Id == id && ar.UserId == userId);

        if (recommendation == null)
            throw new KeyNotFoundException("Recommendation not found.");

        recommendation.IsRead = true;
        await _context.SaveChangesAsync();
    }

    private async Task<AiRecommendationDto> SaveRecommendationAsync(
        string userId, TriggerType triggerType, string text, DateTime? expiresAt = null)
    {
        var recommendation = new AiRecommendation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TriggerType = triggerType,
            RecommendationText = text,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRead = false
        };

        _context.AiRecommendations.Add(recommendation);
        await _context.SaveChangesAsync();

        return MapToDto(recommendation);
    }

    private static AiRecommendationDto MapToDto(AiRecommendation ar) => new()
    {
        Id = ar.Id,
        TriggerType = ar.TriggerType,
        RecommendationText = ar.RecommendationText,
        CreatedAt = ar.CreatedAt,
        ExpiresAt = ar.ExpiresAt,
        IsRead = ar.IsRead
    };
}