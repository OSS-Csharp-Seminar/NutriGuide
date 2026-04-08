using System.ComponentModel.DataAnnotations.Schema;

namespace NutriGuide.Domain.Models;

public class MealLogFavorite
{
    public Guid MealLogId { get; set; }

    [ForeignKey("MealLogId")]
    public MealLog MealLog { get; set; } = null!;

    public Guid FavoriteMealId { get; set; }

    [ForeignKey("FavoriteMealId")]
    public FavoriteMeal FavoriteMeal { get; set; } = null!;
}