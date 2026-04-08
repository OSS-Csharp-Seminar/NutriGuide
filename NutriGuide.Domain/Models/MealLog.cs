using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Domain.Models;

public class MealLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RawInput { get; set; } = string.Empty;

    public MealType? MealType { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public int? Calories { get; set; }
    public decimal? Protein_g { get; set; }
    public decimal? Carbs_g { get; set; }
    public decimal? Fat_g { get; set; }
    public decimal? Fiber_g { get; set; }

    public string? AiNote { get; set; }
    public MealSource Source { get; set; } = MealSource.Manual;

    public ICollection<MealLogFavorite> MealLogFavorites { get; set; } = new List<MealLogFavorite>();
}