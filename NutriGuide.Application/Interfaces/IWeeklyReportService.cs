using NutriGuide.Application.DTOs.WeeklyReport;

namespace NutriGuide.Application.Interfaces;

public interface IWeeklyReportService
{
    Task<WeeklyReportDto> GetWeekAsync(string userId, DateOnly anyDateInWeek);
    Task LogWeightAsync(string userId, DateOnly anyDateInWeek, decimal weightKg);
    Task<string> GenerateSummaryAsync(string userId, DateOnly anyDateInWeek);
}