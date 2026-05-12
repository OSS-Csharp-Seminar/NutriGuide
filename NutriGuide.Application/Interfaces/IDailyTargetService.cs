using NutriGuide.Application.DTOs.DailyTarget;

namespace NutriGuide.Application.Interfaces;

public interface IDailyTargetService
{
    Task<DailyTargetDto> GetOrCreateTodayAsync(string userId);
    Task<DailyTargetDto> UpdateAsync(string userId, UpdateDailyTargetDto dto);
    Task<DailyTargetDto> RegenerateAsync(string userId);
}