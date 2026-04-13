using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.DTOs.UserProfile;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public decimal Height_cm { get; set; }
    public decimal Weight_kg { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public Goal Goal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
   
}