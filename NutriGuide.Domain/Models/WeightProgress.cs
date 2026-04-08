using System.ComponentModel.DataAnnotations;


namespace NutriGuide.Domain.Models;

public class WeightProgress
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateOnly RecordedDate { get; set; }

    [Required]
    public decimal Weight_kg { get; set; }

    public string? Note { get; set; }
}