using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Web.Components;

public abstract class RequireProfile : ComponentBase
{
    [Inject] protected IUserProfileService ProfileService { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected string? UserId { get; private set; }
    protected bool ProfileReady { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthProvider.GetAuthenticationStateAsync();
        UserId = state.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (UserId is null) return;

        if (!await ProfileService.ProfileExistsAsync(UserId))
        {
            Nav.NavigateTo("/profile", forceLoad: false);
            return;
        }

        ProfileReady = true;
        await OnProfileReadyAsync();
    }

    // Profile-dependent pages override this instead of OnInitializedAsync
    protected virtual Task OnProfileReadyAsync() => Task.CompletedTask;
}