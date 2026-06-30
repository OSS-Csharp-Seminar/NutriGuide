using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.Interfaces;

public interface IBotAccountLinkService
{
    string CreateLinkCode(string userId);
    Task<string?> LinkExternalAccountAsync(MessagingPlatform platform, string externalUserId, string linkCode);
    Task<string?> GetUserIdAsync(MessagingPlatform platform, string externalUserId);
}
