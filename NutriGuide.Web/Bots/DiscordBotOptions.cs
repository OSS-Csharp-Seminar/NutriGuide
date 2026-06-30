namespace NutriGuide.Web.Bots;

public class DiscordBotOptions
{
    public string? Token { get; set; }
    public ulong? GuildId { get; set; }
    public bool RegisterCommands { get; set; } = true;
}
