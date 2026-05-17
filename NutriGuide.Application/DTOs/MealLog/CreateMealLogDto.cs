using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.MealLog;

public class CreateMealLogDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Meal description must be at least 3 characters long.")]
    public string RawInput { get; set; } = string.Empty;

    public MealType? MealType { get; set; }
}