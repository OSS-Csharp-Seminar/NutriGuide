using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;

namespace NutriGuide.Web.Bots;

public class DiscordBotHostedService : IHostedService, IDisposable
{
    private const int DiscordMessageLimit = 2000;
    private readonly DiscordSocketClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DiscordBotOptions _options;
    private readonly ILogger<DiscordBotHostedService> _logger;
    private bool _commandsRegistered;

    public DiscordBotHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<DiscordBotOptions> options,
        ILogger<DiscordBotHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            _logger.LogWarning("Discord bot is disabled because DiscordBot:Token is not configured.");
            return;
        }

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.SlashCommandExecuted += SlashCommandExecutedAsync;

        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Log -= LogAsync;
        _client.Ready -= ReadyAsync;
        _client.SlashCommandExecuted -= SlashCommandExecutedAsync;

        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private async Task ReadyAsync()
    {
        if (!_options.RegisterCommands || _commandsRegistered)
            return;

        _commandsRegistered = true;

        foreach (var command in BuildCommands())
        {
            try
            {
                if (_options.GuildId.HasValue)
                {
                    var guild = _client.GetGuild(_options.GuildId.Value);
                    if (guild == null)
                    {
                        _logger.LogWarning("Discord guild {GuildId} was not found. Check DiscordBot:GuildId.", _options.GuildId.Value);
                        return;
                    }

                    await guild.CreateApplicationCommandAsync(command);
                }
                else
                {
                    await _client.CreateGlobalApplicationCommandAsync(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not register Discord slash command {CommandName}. It may already exist.", command.Name.Value);
            }
        }
    }

    private async Task SlashCommandExecutedAsync(SocketSlashCommand command)
    {
        var ephemeral = command.GuildId.HasValue;
        await command.DeferAsync(ephemeral: ephemeral);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var botCommandService = scope.ServiceProvider.GetRequiredService<IBotCommandService>();
            var externalUserId = command.User.Id.ToString();

            var response = command.Data.Name switch
            {
                "link" => await botCommandService.LinkAsync(
                    MessagingPlatform.Discord,
                    externalUserId,
                    GetOption(command, "code")),
                "meal" => await botCommandService.LogMealAsync(
                    MessagingPlatform.Discord,
                    externalUserId,
                    GetOption(command, "text")),
                "feeling" => await botCommandService.LogFeelingAsync(
                    MessagingPlatform.Discord,
                    externalUserId,
                    GetOption(command, "text")),
                _ => "Unknown command."
            };

            await command.FollowupAsync(TrimForDiscord(response), ephemeral: ephemeral);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord command {CommandName} failed.", command.Data.Name);
            await command.FollowupAsync("Something went wrong while processing that command. Please try again.", ephemeral: ephemeral);
        }
    }

    private static IReadOnlyCollection<SlashCommandProperties> BuildCommands()
    {
        var link = new SlashCommandBuilder()
            .WithName("link")
            .WithDescription("Link this Discord account to NutriGuide.")
            .AddOption("code", ApplicationCommandOptionType.String, "The link code from NutriGuide.", isRequired: true)
            .Build();

        var meal = new SlashCommandBuilder()
            .WithName("meal")
            .WithDescription("Log what you ate today.")
            .AddOption("text", ApplicationCommandOptionType.String, "What you ate, for example: banana and greek yogurt.", isRequired: true)
            .Build();

        var feeling = new SlashCommandBuilder()
            .WithName("feeling")
            .WithDescription("Log how you are feeling and get an AI meal suggestion.")
            .AddOption("text", ApplicationCommandOptionType.String, "How you feel, for example: tired and bloated.", isRequired: true)
            .Build();

        return [link, meal, feeling];
    }

    private static string GetOption(SocketSlashCommand command, string optionName)
    {
        return command.Data.Options
            .FirstOrDefault(option => option.Name == optionName)
            ?.Value
            ?.ToString()
            ?.Trim() ?? string.Empty;
    }

    private static string TrimForDiscord(string message)
    {
        if (message.Length <= DiscordMessageLimit)
            return message;

        return message[..(DiscordMessageLimit - 20)] + "...";
    }

    private Task LogAsync(LogMessage message)
    {
        var logLevel = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, message.Exception, "{DiscordMessage}", message.Message);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
