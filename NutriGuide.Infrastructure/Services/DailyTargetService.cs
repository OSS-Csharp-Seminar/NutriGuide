using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.DailyTarget;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class DailyTargetService : IDailyTargetService
{
    private readonly AppDbContext _context;

    public DailyTargetService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DailyTargetDto> GetOrCreateTodayAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = await _context.DailyTargets
            .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.TargetDate == today);

        if (existing != null)
            return MapToDto(existing);

        
        return await RegenerateAsync(userId);
    }

    public async Task<DailyTargetDto> UpdateAsync(string userId, UpdateDailyTargetDto dto)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var target = await _context.DailyTargets
            .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.TargetDate == today);

        if (target == null)
            throw new KeyNotFoundException("Daily target not found.");

        if (dto.Calories.HasValue) target.Calories = dto.Calories.Value;
        if (dto.Protein_g.HasValue) target.Protein_g = dto.Protein_g.Value;
        if (dto.Carbs_g.HasValue) target.Carbs_g = dto.Carbs_g.Value;
        if (dto.Fat_g.HasValue) target.Fat_g = dto.Fat_g.Value;
        if (dto.Fiber_g.HasValue) target.Fiber_g = dto.Fiber_g.Value;

        target.IsAiGenerated = false;

        await _context.SaveChangesAsync();

        return MapToDto(target);
    }

    public async Task<DailyTargetDto> RegenerateAsync(string userId)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            throw new KeyNotFoundException("Profile not found. Create profile before daily target generation.");

        var targets = CalculateTargets(profile);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        
        var existing = await _context.DailyTargets
            .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.TargetDate == today);

        if (existing != null)
        {
            existing.Calories = targets.Calories;
            existing.Protein_g = targets.Protein_g;
            existing.Carbs_g = targets.Carbs_g;
            existing.Fat_g = targets.Fat_g;
            existing.Fiber_g = targets.Fiber_g;
            existing.IsAiGenerated = true;
            await _context.SaveChangesAsync();
            return MapToDto(existing);
        }

        var newTarget = new DailyTarget
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetDate = today,
            Calories = targets.Calories,
            Protein_g = targets.Protein_g,
            Carbs_g = targets.Carbs_g,
            Fat_g = targets.Fat_g,
            Fiber_g = targets.Fiber_g,
            IsAiGenerated = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.DailyTargets.Add(newTarget);
        await _context.SaveChangesAsync();

        return MapToDto(newTarget);
    }

    
    private static (int Calories, decimal Protein_g, decimal Carbs_g, decimal Fat_g, decimal Fiber_g)
        CalculateTargets(UserProfile profile)
    {
        double bmr = profile.Gender == Gender.Male
            ? 10 * (double)profile.Weight_kg + 6.25 * (double)profile.Height_cm - 5 * profile.Age + 5
            : 10 * (double)profile.Weight_kg + 6.25 * (double)profile.Height_cm - 5 * profile.Age - 161;

        double activityMultiplier = profile.ActivityLevel switch
        {
            ActivityLevel.Light => 1.375,
            ActivityLevel.Moderate => 1.55,
            ActivityLevel.Active => 1.725,
            ActivityLevel.VeryActive => 1.9,
            _ => 1.375
        };

        double tdee = bmr * activityMultiplier;

        double targetCalories = profile.Goal switch
        {
            Goal.Loss => tdee - 500,        
            Goal.Gain => tdee + 300,        
            Goal.Maintenance => tdee,
            _ => tdee
        };

        targetCalories = Math.Max(1200, targetCalories); // minimum 1200 kcal

        // Korak 4 — Makronutrijenti
        // Proteini: 2g po kg tjelesne težine
        // Masti: 25% ukupnih kalorija
        // Ugljikohidrati: ostatak kalorija
        double protein = (double)profile.Weight_kg * 2.0;
        double fat = targetCalories * 0.25 / 9;
        double carbs = (targetCalories - protein * 4 - fat * 9) / 4;

        // Korak 5 — Vlakna (preporuka: 14g na 1000 kcal)
        double fiber = targetCalories / 1000 * 14;

        return (
            (int)Math.Round(targetCalories),
            (decimal)Math.Round(protein, 1),
            (decimal)Math.Round(carbs, 1),
            (decimal)Math.Round(fat, 1),
            (decimal)Math.Round(fiber, 1)
        );
    }

    private static DailyTargetDto MapToDto(DailyTarget target) => new()
    {
        Id = target.Id,
        TargetDate = target.TargetDate,
        Calories = target.Calories,
        Protein_g = target.Protein_g,
        Carbs_g = target.Carbs_g,
        Fat_g = target.Fat_g,
        Fiber_g = target.Fiber_g,
        IsAiGenerated = target.IsAiGenerated
    };
}