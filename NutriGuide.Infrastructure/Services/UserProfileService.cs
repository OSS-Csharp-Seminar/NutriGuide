using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.UserProfile;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _context;
    
    public UserProfileService(AppDbContext context)
    {
        _context = context; 
    }

    public async Task<UserProfileDto> CreateAsync(string userId, CreateUserProfileDto dto)
    {
        var existingProfile = await  _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (existingProfile != null)
            throw new Exception("Profile already exists");

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Gender = dto.Gender,
            Age = dto.Age,
            Height_cm = dto.Height_cm,
            Weight_kg = dto.Weight_kg,
            ActivityLevel = dto.ActivityLevel,
            Goal = dto.Goal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow

        };
        
        _context.UserProfiles.Add(profile);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _context.WeightProgress.Add(new WeightProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RecordedDate = today,
            Weight_kg = dto.Weight_kg
        });

        await _context.SaveChangesAsync();

        return MapToDto(profile);
    }


    public async Task<UserProfileDto> GetByUserIdAsync(string userId)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (profile == null)
            throw new KeyNotFoundException("Profile not found");
        
        return MapToDto(profile);
    }


    public async Task<UserProfileDto> UpdateAsync(string userId, UpdateUserProfileDto dto)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
        if (profile == null)
            throw new KeyNotFoundException("Profile not found");
        
        if (dto.Age.HasValue) profile.Age = dto.Age.Value;
        if (dto.Height_cm.HasValue) profile.Height_cm = dto.Height_cm.Value;
        if (dto.Weight_kg.HasValue) profile.Weight_kg = dto.Weight_kg.Value;
        if (dto.ActivityLevel.HasValue) profile.ActivityLevel = dto.ActivityLevel.Value;
        if (dto.Goal.HasValue) profile.Goal = dto.Goal.Value;
        
        profile.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return MapToDto(profile);
    }

    public async Task<bool> ProfileExistsAsync(string userId)
    {
        return await _context.UserProfiles.AnyAsync(p => p.UserId == userId);
    }

    private static UserProfileDto MapToDto(UserProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        Gender = profile.Gender,
        Age = profile.Age,
        Height_cm = profile.Height_cm,
        Weight_kg = profile.Weight_kg,
        ActivityLevel = profile.ActivityLevel,
        Goal = profile.Goal,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };




}