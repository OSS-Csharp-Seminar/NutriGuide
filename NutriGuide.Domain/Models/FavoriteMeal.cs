using System.ComponentModel.DataAnnotations;


namespace NutriGuide.Domain.Models;

public class FavoriteMeal
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? Calories { get; set; }
    public decimal? Protein_g { get; set; }
    public decimal? Carbs_g { get; set; }
    public decimal? Fat_g { get; set; }
    public decimal? Fiber_g { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MealLogFavorite> MealLogFavorites { get; set; } = new List<MealLogFavorite>();
}