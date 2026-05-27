using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Controllers;

[Authorize]
public class AiRecommendationController : BaseController
{
    private readonly IAiRecommendationService _recommendationService;

    public AiRecommendationController(IAiRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var recommendations = await _recommendationService.GetUnreadAsync(UserId);
        return Ok(recommendations);
    }

    [HttpPost("next-meal")]
    public async Task<IActionResult> GenerateNextMeal()
    {
        var recommendation = await _recommendationService.GenerateNextMealAsync(UserId);
        return Ok(recommendation);
    }

    [HttpPost("target-miss")]
    public async Task<IActionResult> GenerateTargetMiss()
    {
        var recommendation = await _recommendationService.GenerateTargetMissAsync(UserId);
        return Ok(recommendation);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _recommendationService.MarkAsReadAsync(UserId, id);
        return NoContent();
    }
}