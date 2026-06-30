using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Controllers;

[Authorize]
public class BotLinkController : BaseController
{
    private readonly IBotAccountLinkService _botAccountLinkService;

    public BotLinkController(IBotAccountLinkService botAccountLinkService)
    {
        _botAccountLinkService = botAccountLinkService;
    }

    [HttpPost("discord")]
    public IActionResult CreateDiscordLinkCode()
    {
        var code = _botAccountLinkService.CreateLinkCode(UserId);

        return Ok(new
        {
            Code = code,
            ExpiresInMinutes = 10,
            DiscordCommand = $"/link code:{code}"
        });
    }
}
