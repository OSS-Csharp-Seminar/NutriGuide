using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace NutriGuide.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}