using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGuide.Application.DTOs.DailyTarget;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Controllers;

[Authorize]
public class DailyTargetController : BaseController
{
    private readonly IDailyTargetService _dailyTargetService;

    public DailyTargetController(IDailyTargetService dailyTargetService)
    {
        _dailyTargetService = dailyTargetService;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var target = await _dailyTargetService.GetOrCreateTodayAsync(UserId);
        return Ok(target);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDailyTargetDto dto)
    {
        var target = await _dailyTargetService.UpdateAsync(UserId, dto);
        return Ok(target);
    }

    [HttpPost("regenerate")]
    public async Task<IActionResult> Regenerate()
    {
        var target = await _dailyTargetService.RegenerateAsync(UserId);
        return Ok(target);
    }
}