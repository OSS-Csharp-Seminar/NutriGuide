using NutriGuide.Application.DTOs.UserProfile;
using NutriGuide.Domain.Models;

namespace NutriGuide.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto> CreateAsync(string userId, CreateUserProfileDto dto);
    Task<UserProfileDto> GetByUserIdAsync(string userId);
    Task<UserProfileDto> UpdateAsync(string userId, UpdateUserProfileDto dto);
    Task<bool> ProfileExistsAsync(string userId);
}