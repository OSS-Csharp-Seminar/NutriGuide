using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.FavoriteMeal;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class FavoriteMealService : IFavoriteMealService
{
    private readonly AppDbContext _context;

    public FavoriteMealService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FavoriteMealDto>> GetAllAsync(string userId)
    {
        return await _context.FavoriteMeals
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Name)
            .Select(f => MapToDto(f))
            .ToListAsync();
    }

    public async Task<FavoriteMealDto> SaveFromMealAsync(string userId, Guid mealLogId)
    {
        var meal = await _context.MealLogs
            .FirstOrDefaultAsync(ml => ml.Id == mealLogId && ml.UserId == userId);

        if (meal == null)
            throw new KeyNotFoundException("Meal not found.");

        // Avoid duplicate favorites with the same name
        var existing = await _context.FavoriteMeals
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Name == meal.RawInput);

        if (existing != null)
            throw new InvalidOperationException("This meal is already in your favorites.");

        var favorite = new FavoriteMeal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = meal.RawInput,
            Description = meal.RawInput,
            Calories = meal.Calories,
            Protein_g = meal.Protein_g,
            Carbs_g = meal.Carbs_g,
            Fat_g = meal.Fat_g,
            Fiber_g = meal.Fiber_g,
            SavedAt = DateTime.UtcNow
        };

        _context.FavoriteMeals.Add(favorite);
        await _context.SaveChangesAsync();

        return MapToDto(favorite);
    }

    public async Task DeleteAsync(string userId, Guid favoriteId)
    {
        var favorite = await _context.FavoriteMeals
            .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

        if (favorite == null)
            throw new KeyNotFoundException("Favorite not found.");

        _context.FavoriteMeals.Remove(favorite);
        await _context.SaveChangesAsync();
    }

    public async Task LogFromFavoriteAsync(string userId, Guid favoriteId)
    {
        var favorite = await _context.FavoriteMeals
            .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

        if (favorite == null)
            throw new KeyNotFoundException("Favorite not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var mealLog = new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RawInput = favorite.Name,
            LoggedAt = DateTime.UtcNow,
            Calories = favorite.Calories,
            Protein_g = favorite.Protein_g,
            Carbs_g = favorite.Carbs_g,
            Fat_g = favorite.Fat_g,
            Fiber_g = favorite.Fiber_g,
            AiNote = "Logged from favorites",
            Source = MealSource.Favorite
        };

        _context.MealLogs.Add(mealLog);

        // Update today's running summary, mirroring MealLogService
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

        summary.TotalCalories += mealLog.Calories ?? 0;
        summary.TotalProtein_g += mealLog.Protein_g ?? 0;
        summary.TotalCarbs_g += mealLog.Carbs_g ?? 0;
        summary.TotalFat_g += mealLog.Fat_g ?? 0;
        summary.TotalFiber_g += mealLog.Fiber_g ?? 0;
        summary.MealCount += 1;
        summary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static FavoriteMealDto MapToDto(FavoriteMeal f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        Calories = f.Calories,
        Protein_g = f.Protein_g,
        Carbs_g = f.Carbs_g,
        Fat_g = f.Fat_g,
        Fiber_g = f.Fiber_g
    };
}