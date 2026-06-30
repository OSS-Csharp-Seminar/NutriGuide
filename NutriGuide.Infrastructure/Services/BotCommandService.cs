using Microsoft.EntityFrameworkCore;
using NutriGuide.Application.DTOs.MealLog;
using NutriGuide.Application.DTOs.Wellness;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class BotCommandService : IBotCommandService
{
    private readonly AppDbContext _context;
    private readonly IBotAccountLinkService _accountLinkService;
    private readonly IMealLogService _mealLogService;
    private readonly IWellnessService _wellnessService;

    public BotCommandService(
        AppDbContext context,
        IBotAccountLinkService accountLinkService,
        IMealLogService mealLogService,
        IWellnessService wellnessService)
    {
        _context = context;
        _accountLinkService = accountLinkService;
        _mealLogService = mealLogService;
        _wellnessService = wellnessService;
    }

    public async Task<string> LinkAsync(MessagingPlatform platform, string externalUserId, string linkCode)
    {
        if (string.IsNullOrWhiteSpace(linkCode))
            return "Please provide the link code from NutriGuide.";

        var userId = await _accountLinkService.LinkExternalAccountAsync(platform, externalUserId, linkCode);

        return userId == null
            ? "That link code is invalid or expired. Create a new code in NutriGuide and try again."
            : "Your Discord account is now linked to NutriGuide.";
    }

    public async Task<string> LogMealAsync(MessagingPlatform platform, string externalUserId, string rawInput)
    {
        rawInput = rawInput.Trim();
        if (rawInput.Length < 3)
            return "Tell me what you ate, for example: /meal text:banana and greek yogurt";

        var connection = await GetConnectionAsync(platform, externalUserId);
        if (connection == null)
            return NotLinkedMessage();

        var messageLog = await CreateMessageLogAsync(connection, rawInput);

        try
        {
            var meal = await _mealLogService.CreateAsync(connection.UserId, new CreateMealLogDto
            {
                RawInput = rawInput,
                Source = MealSource.Messaging
            });

            messageLog.MealLogId = meal.Id;
            messageLog.IsProcessed = true;
            await _context.SaveChangesAsync();

            return $"""
                   Logged your meal: {meal.RawInput}

                   Estimated:
                   Calories: {meal.Calories ?? 0} kcal
                   Protein: {meal.Protein_g ?? 0}g
                   Carbs: {meal.Carbs_g ?? 0}g
                   Fat: {meal.Fat_g ?? 0}g
                   Fiber: {meal.Fiber_g ?? 0}g

                   AI note: {meal.AiNote}
                   """;
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(messageLog, ex);
            throw;
        }
    }

    public async Task<string> LogFeelingAsync(MessagingPlatform platform, string externalUserId, string symptoms)
    {
        symptoms = symptoms.Trim();
        if (symptoms.Length < 3)
            return "Tell me how you feel, for example: /feeling text:tired and bloated";

        var connection = await GetConnectionAsync(platform, externalUserId);
        if (connection == null)
            return NotLinkedMessage();

        var messageLog = await CreateMessageLogAsync(connection, symptoms);

        try
        {
            var wellnessLog = await _wellnessService.CreateAsync(connection.UserId, new CreateWellnessLogDto
            {
                Symptoms = symptoms
            });

            messageLog.IsProcessed = true;
            await _context.SaveChangesAsync();

            return $"""
                   Logged how you feel: {wellnessLog.Symptoms}

                   AI analysis: {wellnessLog.AiAnalysis}

                   Suggested meal: {wellnessLog.SuggestedMeal}
                   """;
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(messageLog, ex);
            throw;
        }
    }

    private async Task<MessagingConnection?> GetConnectionAsync(MessagingPlatform platform, string externalUserId)
    {
        return await _context.MessagingConnections
            .FirstOrDefaultAsync(connection =>
                connection.Platform == platform &&
                connection.ExternalUserId == externalUserId &&
                connection.IsActive);
    }

    private async Task<MessagingLog> CreateMessageLogAsync(MessagingConnection connection, string rawMessage)
    {
        var messageLog = new MessagingLog
        {
            Id = Guid.NewGuid(),
            ConnectionId = connection.Id,
            RawMessage = rawMessage,
            ReceivedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        _context.MessagingLogs.Add(messageLog);
        await _context.SaveChangesAsync();

        return messageLog;
    }

    private async Task MarkFailedAsync(MessagingLog messageLog, Exception ex)
    {
        messageLog.IsProcessed = false;
        messageLog.ProcessingError = ex.Message;
        await _context.SaveChangesAsync();
    }

    private static string NotLinkedMessage()
    {
        return "Your Discord account is not linked yet. In NutriGuide, create a Discord link code, then run /link with that code.";
    }
}
