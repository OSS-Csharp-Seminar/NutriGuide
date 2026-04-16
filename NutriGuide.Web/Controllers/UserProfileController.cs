using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGuide.Application.DTOs.UserProfile;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Controllers;


[Authorize]
public class UserProfileController : BaseController
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;   
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserProfileDto dto)
    {
        var profile = await _userProfileService.CreateAsync(UserId,dto);
        return CreatedAtAction(nameof(Get), profile);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await _userProfileService.GetByUserIdAsync(UserId);
        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserProfileDto dto)
    {
        var profile = await _userProfileService.UpdateAsync(UserId, dto);
        return Ok(profile);
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists()
    {
        var exists = await _userProfileService.ProfileExistsAsync(UserId);
        return Ok(new { exists });
    }
    
    
}