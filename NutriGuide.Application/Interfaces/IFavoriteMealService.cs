using NutriGuide.Application.DTOs.FavoriteMeal;

namespace NutriGuide.Application.Interfaces;

public interface IFavoriteMealService
{
    Task<List<FavoriteMealDto>> GetAllAsync(string userId);
    Task<FavoriteMealDto> SaveFromMealAsync(string userId, Guid mealLogId);
    Task DeleteAsync(string userId, Guid favoriteId);
    Task LogFromFavoriteAsync(string userId, Guid favoriteId);
}