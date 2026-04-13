using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.UserProfile;

public class CreateUserProfileDto
{
     
    [Required]
    public Gender Gender { get; set; }
    
    [Required]
    [Range(13,120)]
    public int Age { get; set; }
    
    [Required]
    [Range(50,251)]
    public decimal Height_cm { get; set; }
    
    [Required]
    [Range(20,450)]
    public decimal Weight_kg { get; set; }
    
    [Required]
    public ActivityLevel ActivityLevel { get; set; }
    
    [Required]
    public Goal Goal { get; set; }
    
}