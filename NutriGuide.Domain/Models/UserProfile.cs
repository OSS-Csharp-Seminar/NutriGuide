using System.ComponentModel.DataAnnotations;
using NutriGuide.Domain.Enums;


namespace NutriGuide.Domain.Models;


public class UserProfile
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public Gender Gender { get; set; }
    
    [Required]
    public int Age { get; set; }
    
    [Required]
    public decimal Height_cm { get; set; }

    [Required]
    public decimal Weight_kg { get; set; }

    [Required]
    public ActivityLevel ActivityLevel { get; set; }

    [Required]
    public Goal Goal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}