using NutriGuide.Application.DTOs.Wellness;

namespace NutriGuide.Application.Interfaces;

public interface IWellnessService
{
    Task<WellnessLogDto> CreateAsync(string userId, CreateWellnessLogDto dto);
    Task<List<WellnessLogDto>> GetHistoryAsync(string userId);
}