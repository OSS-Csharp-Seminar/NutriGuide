using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGuide.Application.DTOs.Wellness;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Controllers;

[Authorize]
public class WellnessController : BaseController
{
    private readonly IWellnessService _wellnessService;

    public WellnessController(IWellnessService wellnessService)
    {
        _wellnessService = wellnessService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWellnessLogDto dto)
    {
        var log = await _wellnessService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetHistory), log);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _wellnessService.GetHistoryAsync(UserId);
        return Ok(history);
    }
}