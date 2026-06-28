using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NutriGuide.Web.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager,
                            UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromForm] string email, [FromForm] string password, [FromForm] string? returnUrl)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Redirect("/login?error=1");

        var result = await _signInManager.PasswordSignInAsync(
            user, password, isPersistent: true, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Redirect("/login?error=1");

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromForm] string email, [FromForm] string userName, [FromForm] string password)
    {
        var user = new IdentityUser { Email = email, UserName = userName };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var msg = Uri.EscapeDataString(
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return Redirect($"/register?error={msg}");
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        return LocalRedirect("/profile");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/login");
    }
}