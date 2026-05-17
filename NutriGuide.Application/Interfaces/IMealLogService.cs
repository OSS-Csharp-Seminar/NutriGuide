using NutriGuide.Application.DTOs.MealLog;

namespace NutriGuide.Application.Interfaces;

public interface IMealLogService
{
    Task<MealLogDto> CreateAsync(string userId, CreateMealLogDto dto);
    Task<DailyMealSummaryDto> GetTodayAsync(string userId);
    Task<DailyMealSummaryDto> GetByDateAsync(string userId, DateOnly date);
    Task DeleteAsync(string userId, Guid id);
}