using NutriGuide.Domain.Enums;

namespace NutriGuide.Application.Interfaces;

public interface IBotCommandService
{
    Task<string> LinkAsync(MessagingPlatform platform, string externalUserId, string linkCode);
    Task<string> LogMealAsync(MessagingPlatform platform, string externalUserId, string rawInput);
    Task<string> LogFeelingAsync(MessagingPlatform platform, string externalUserId, string symptoms);
}
