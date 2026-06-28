namespace NutriGuide.Application.DTOs.FavoriteMeal;

public class FavoriteMealDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Calories { get; set; }
    public decimal? Protein_g { get; set; }
    public decimal? Carbs_g { get; set; }
    public decimal? Fat_g { get; set; }
    public decimal? Fiber_g { get; set; }
}