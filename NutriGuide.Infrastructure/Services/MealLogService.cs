using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.MealLog;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class MealLogService : IMealLogService
{
    private readonly AppDbContext _context;
    private readonly IAiService _aiService;

    public MealLogService(AppDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<MealLogDto> CreateAsync(string userId, CreateMealLogDto dto)
    {
        
        var nutrition = await _aiService.EstimateNutritionAsync(dto.RawInput);

        var mealLog = new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RawInput = dto.RawInput,
            MealType = dto.MealType,
            LoggedAt = DateTime.UtcNow,
            Calories = nutrition.Calories,
            Protein_g = nutrition.Protein_g,
            Carbs_g = nutrition.Carbs_g,
            Fat_g = nutrition.Fat_g,
            Fiber_g = nutrition.Fiber_g,
            AiNote = nutrition.AiNote,
            Source = Domain.Enums.MealSource.Manual
        };

        _context.MealLogs.Add(mealLog);

        
        await UpdateDailyNutritionSummaryAsync(userId, mealLog);

        await _context.SaveChangesAsync();

        return MapToDto(mealLog);
    }

    public async Task<DailyMealSummaryDto> GetTodayAsync(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await GetByDateAsync(userId, today);
    }

    public async Task<DailyMealSummaryDto> GetByDateAsync(string userId, DateOnly date)
    {
        var meals = await _context.MealLogs
            .Where(ml => ml.UserId == userId &&
                         DateOnly.FromDateTime(ml.LoggedAt) == date)
            .OrderBy(ml => ml.LoggedAt)
            .ToListAsync();

        var summary = await _context.DailyNutritionSummaries
            .FirstOrDefaultAsync(dns => dns.UserId == userId && dns.SummaryDate == date);

        return new DailyMealSummaryDto
        {
            Date = date,
            Meals = meals.Select(MapToDto).ToList(),
            TotalCalories = summary?.TotalCalories ?? 0,
            TotalProtein_g = summary?.TotalProtein_g ?? 0,
            TotalCarbs_g = summary?.TotalCarbs_g ?? 0,
            TotalFat_g = summary?.TotalFat_g ?? 0,
            TotalFiber_g = summary?.TotalFiber_g ?? 0,
            MealCount = summary?.MealCount ?? meals.Count
        };
    }

    public async Task DeleteAsync(string userId, Guid id)
    {
        var mealLog = await _context.MealLogs
            .FirstOrDefaultAsync(ml => ml.Id == id && ml.UserId == userId);

        if (mealLog == null)
            throw new KeyNotFoundException("Meal not found.");

        _context.MealLogs.Remove(mealLog);
        
        await RecalculateDailyNutritionSummaryAsync(userId,
            DateOnly.FromDateTime(mealLog.LoggedAt));

        await _context.SaveChangesAsync();
    }
    

    private async Task UpdateDailyNutritionSummaryAsync(string userId, MealLog newMeal)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var summary = await _context.DailyNutritionSummaries
            .FirstOrDefaultAsync(dns => dns.UserId == userId && dns.SummaryDate == today);

        if (summary == null)
        {
            summary = new DailyNutritionSummary
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SummaryDate = today
            };
            _context.DailyNutritionSummaries.Add(summary);
        }

        summary.TotalCalories += newMeal.Calories ?? 0;
        summary.TotalProtein_g += newMeal.Protein_g ?? 0;
        summary.TotalCarbs_g += newMeal.Carbs_g ?? 0;
        summary.TotalFat_g += newMeal.Fat_g ?? 0;
        summary.TotalFiber_g += newMeal.Fiber_g ?? 0;
        summary.MealCount += 1;
        summary.UpdatedAt = DateTime.UtcNow;
    }

    private async Task RecalculateDailyNutritionSummaryAsync(string userId, DateOnly date)
    {
        var summary = await _context.DailyNutritionSummaries
            .FirstOrDefaultAsync(dns => dns.UserId == userId && dns.SummaryDate == date);

        if (summary == null) return;

        var meals = await _context.MealLogs
            .Where(ml => ml.UserId == userId &&
                         DateOnly.FromDateTime(ml.LoggedAt) == date)
            .ToListAsync();

        summary.TotalCalories = meals.Sum(m => m.Calories ?? 0);
        summary.TotalProtein_g = meals.Sum(m => m.Protein_g ?? 0);
        summary.TotalCarbs_g = meals.Sum(m => m.Carbs_g ?? 0);
        summary.TotalFat_g = meals.Sum(m => m.Fat_g ?? 0);
        summary.TotalFiber_g = meals.Sum(m => m.Fiber_g ?? 0);
        summary.MealCount = meals.Count;
        summary.UpdatedAt = DateTime.UtcNow;
    }

    private static MealLogDto MapToDto(MealLog meal) => new()
    {
        Id = meal.Id,
        RawInput = meal.RawInput,
        MealType = meal.MealType,
        LoggedAt = meal.LoggedAt,
        Calories = meal.Calories,
        Protein_g = meal.Protein_g,
        Carbs_g = meal.Carbs_g,
        Fat_g = meal.Fat_g,
        Fiber_g = meal.Fiber_g,
        AiNote = meal.AiNote,
        Source = meal.Source
    };
}