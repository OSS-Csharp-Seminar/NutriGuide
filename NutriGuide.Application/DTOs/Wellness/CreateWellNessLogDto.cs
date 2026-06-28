using System.ComponentModel.DataAnnotations;

namespace NutriGuide.Application.DTOs.Wellness;

public class CreateWellnessLogDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Opis simptoma mora imati najmanje 3 znaka.")]
    public string Symptoms { get; set; } = string.Empty;
}