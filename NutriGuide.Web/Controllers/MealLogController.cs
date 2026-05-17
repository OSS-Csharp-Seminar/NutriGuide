using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGuide.Application.DTOs.MealLog;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Controllers;

[Authorize]
public class MealLogController : BaseController
{
    private readonly IMealLogService _mealLogService;

    public MealLogController(IMealLogService mealLogService)
    {
        _mealLogService = mealLogService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMealLogDto dto)
    {
        var meal = await _mealLogService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetToday), meal);
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var summary = await _mealLogService.GetTodayAsync(UserId);
        return Ok(summary);
    }

    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetByDate(DateOnly date)
    {
        var summary = await _mealLogService.GetByDateAsync(UserId, date);
        return Ok(summary);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mealLogService.DeleteAsync(UserId, id);
        return NoContent();
    }
}