using NutriGuide.Application.DTOs.MealLog;

namespace NutriGuide.Application.Interfaces;

public interface IMealLogService
{
    Task<MealLogDto> CreateAsync(string userId, CreateMealLogDto dto);
    Task<DailyMealSummaryDto> GetTodayAsync(string userId);
    Task<DailyMealSummaryDto> GetByDateAsync(string userId, DateOnly date);
    Task<HashSet<DateOnly>> GetLoggedDatesAsync(string userId, DateOnly from, DateOnly to);
    Task DeleteAsync(string userId, Guid id);
}