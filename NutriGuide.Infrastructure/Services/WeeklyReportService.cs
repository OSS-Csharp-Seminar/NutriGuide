using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.WeeklyReport;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class WeeklyReportService : IWeeklyReportService
{
    private readonly AppDbContext _context;
    private readonly IAiService _aiService;

    public WeeklyReportService(AppDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    private static (DateOnly start, DateOnly end) WeekBounds(DateOnly anyDateInWeek)
    {
        var dayOffset = ((int)anyDateInWeek.DayOfWeek + 6) % 7;
        var start = anyDateInWeek.AddDays(-dayOffset);
        return (start, start.AddDays(6));
    }

    public async Task<WeeklyReportDto> GetWeekAsync(string userId, DateOnly anyDateInWeek)
    {
        var (weekStart, weekEnd) = WeekBounds(anyDateInWeek);

        var summaries = await _context.DailyNutritionSummaries
            .Where(s => s.UserId == userId &&
                        s.SummaryDate >= weekStart &&
                        s.SummaryDate <= weekEnd &&
                        s.MealCount > 0)
            .ToListAsync();

        var daysLogged = summaries.Count;

        var dto = new WeeklyReportDto
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            DaysLogged = daysLogged
        };

        if (daysLogged > 0)
        {
            dto.AvgCalories = Math.Round((decimal)summaries.Average(s => s.TotalCalories), 0);
            dto.AvgProtein_g = Math.Round(summaries.Average(s => s.TotalProtein_g), 1);
            dto.AvgCarbs_g = Math.Round(summaries.Average(s => s.TotalCarbs_g), 1);
            dto.AvgFat_g = Math.Round(summaries.Average(s => s.TotalFat_g), 1);
            dto.AvgFiber_g = Math.Round(summaries.Average(s => s.TotalFiber_g), 1);
        }

        var target = await _context.DailyTargets
            .Where(t => t.UserId == userId && t.TargetDate <= weekEnd)
            .OrderByDescending(t => t.TargetDate)
            .FirstOrDefaultAsync();

        if (target != null)
        {
            dto.TargetCalories = target.Calories;
            if (target.Calories > 0 && daysLogged > 0)
                dto.CalorieAdherence_pct =
                    Math.Round(dto.AvgCalories / target.Calories * 100, 0);
        }

        var profile = await _context.UserProfiles
           .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile != null)
        {
            dto.Goal = profile.Goal.ToString();
            var registeredWeekStart = WeekBounds(DateOnly.FromDateTime(profile.CreatedAt)).start;
            dto.CanLogWeight = weekStart >= registeredWeekStart;
        }

        var thisWeekEntry = await _context.WeightProgress
            .Where(w => w.UserId == userId &&
                        w.RecordedDate >= weekStart &&
                        w.RecordedDate <= weekEnd)
            .OrderByDescending(w => w.RecordedDate)
            .FirstOrDefaultAsync();

        if (thisWeekEntry != null && profile != null)
        {
            dto.ThisWeekWeight_kg = thisWeekEntry.Weight_kg;
            dto.StartingWeight_kg = profile.Weight_kg;
            dto.WeightChange_kg = Math.Round(thisWeekEntry.Weight_kg - profile.Weight_kg, 1);

            var change = dto.WeightChange_kg.Value;
            dto.OnTrack = profile.Goal switch
            {
                Goal.Loss => change < 0,
                Goal.Gain => change > 0,
                Goal.Maintenance => Math.Abs(change) <= 1.0m,
                _ => null
            };
        }

        return dto;
    }

    public async Task LogWeightAsync(string userId, DateOnly anyDateInWeek, decimal weightKg)
    {
        var (weekStart, weekEnd) = WeekBounds(anyDateInWeek);

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            throw new InvalidOperationException("Create a profile before logging weight.");

        var registeredWeekStart = WeekBounds(DateOnly.FromDateTime(profile.CreatedAt)).start;

        if (weekStart < registeredWeekStart)
            throw new InvalidOperationException("You can't record weight for a week before you started using the app.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var recordDate = weekEnd > today ? today : weekEnd;

        var existing = await _context.WeightProgress
            .Where(w => w.UserId == userId &&
                        w.RecordedDate >= weekStart &&
                        w.RecordedDate <= weekEnd)
            .ToListAsync();

        if (existing.Count > 0)
        {
            var keep = existing.First();
            keep.Weight_kg = weightKg;
            keep.RecordedDate = recordDate;
            if (existing.Count > 1)
                _context.WeightProgress.RemoveRange(existing.Skip(1));
        }
        else
        {
            _context.WeightProgress.Add(new WeightProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RecordedDate = recordDate,
                Weight_kg = weightKg
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateSummaryAsync(string userId, DateOnly anyDateInWeek)
    {
        var report = await GetWeekAsync(userId, anyDateInWeek);

        if (report.DaysLogged == 0)
            return "No meals were logged this week, so there's nothing to summarize yet.";

        return await _aiService.GenerateWeeklySummaryAsync(report);
    }
}