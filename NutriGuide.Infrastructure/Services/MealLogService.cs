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
            Source = dto.Source ?? Domain.Enums.MealSource.Manual
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

        var mealDate = DateOnly.FromDateTime(mealLog.LoggedAt);

        _context.MealLogs.Remove(mealLog);
        await _context.SaveChangesAsync();

        await RecalculateDailyNutritionSummaryAsync(userId, mealDate);
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

    public async Task<HashSet<DateOnly>> GetLoggedDatesAsync(string userId, DateOnly from, DateOnly to)
    {
        var fromDt = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toDt = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var dates = await _context.MealLogs
            .Where(ml => ml.UserId == userId &&
                         ml.LoggedAt >= fromDt &&
                         ml.LoggedAt < toDt)
            .Select(ml => ml.LoggedAt)
            .ToListAsync();

        return dates.Select(DateOnly.FromDateTime).ToHashSet();
    }

    public async Task<List<DailyNutritionStatsDto>> GetStatsAsync(string userId, DateOnly? from, DateOnly to)
    {
        var summariesQuery = _context.DailyNutritionSummaries
            .Where(s => s.UserId == userId &&
                        s.SummaryDate <= to &&
                        s.MealCount > 0);

        if (from.HasValue)
            summariesQuery = summariesQuery.Where(s => s.SummaryDate >= from.Value);

        var summaries = await summariesQuery
            .OrderBy(s => s.SummaryDate)
            .ToListAsync();

        if (summaries.Count == 0)
            return new List<DailyNutritionStatsDto>();

        var firstSummaryDate = summaries.First().SummaryDate;
        var targets = await _context.DailyTargets
            .Where(t => t.UserId == userId && t.TargetDate <= to)
            .OrderBy(t => t.TargetDate)
            .ToListAsync();

        if (targets.Count == 0)
        {
            targets = await _context.DailyTargets
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.TargetDate)
                .Take(1)
                .ToListAsync();
        }

        var result = new List<DailyNutritionStatsDto>();
        foreach (var summary in summaries)
        {
            var target = targets
                .Where(t => t.TargetDate <= summary.SummaryDate)
                .OrderByDescending(t => t.TargetDate)
                .FirstOrDefault()
                ?? targets.FirstOrDefault(t => t.TargetDate >= firstSummaryDate)
                ?? targets.LastOrDefault();

            result.Add(new DailyNutritionStatsDto
            {
                Date = summary.SummaryDate,
                Calories = summary.TotalCalories,
                Protein_g = summary.TotalProtein_g,
                Carbs_g = summary.TotalCarbs_g,
                Fat_g = summary.TotalFat_g,
                Fiber_g = summary.TotalFiber_g,
                MealCount = summary.MealCount,
                TargetCalories = target?.Calories ?? 0,
                TargetProtein_g = target?.Protein_g ?? 0,
                TargetCarbs_g = target?.Carbs_g ?? 0,
                TargetFat_g = target?.Fat_g ?? 0,
                TargetFiber_g = target?.Fiber_g ?? 0
            });
        }

        return result;
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
