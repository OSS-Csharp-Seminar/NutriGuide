using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.UserProfile;

public class UpdateUserProfileDto
{
    [Range(13,120)]
    public int? Age { get; set; }
    
    [Range(50,251)]
    public decimal? Height_cm { get; set; }
    
    [Range(20,450)]
    public decimal? Weight_kg { get; set; }
    
    public ActivityLevel? ActivityLevel { get; set; }
    
    public Goal? Goal { get; set; }
    
}