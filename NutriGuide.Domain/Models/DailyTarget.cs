using System.ComponentModel.DataAnnotations;

namespace NutriGuide.Domain.Models;

public class DailyTarget
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateOnly TargetDate { get; set; }

    [Required]
    public int Calories { get; set; }

    [Required]
    public decimal Protein_g { get; set; }

    [Required]
    public decimal Carbs_g { get; set; }

    [Required]
    public decimal Fat_g { get; set; }

    [Required]
    public decimal Fiber_g { get; set; }

    public bool IsAiGenerated { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}