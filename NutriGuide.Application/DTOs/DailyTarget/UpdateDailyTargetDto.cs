using System.ComponentModel.DataAnnotations;

namespace NutriGuide.Application.DTOs.DailyTarget;

public class UpdateDailyTargetDto
{
    [Range(500, 10000)]
    public int? Calories { get; set; }

    [Range(0, 500)]
    public decimal? Protein_g { get; set; }

    [Range(0, 500)]
    public decimal? Carbs_g { get; set; }

    [Range(0, 500)]
    public decimal? Fat_g { get; set; }

    [Range(0, 100)]
    public decimal? Fiber_g { get; set; }
}