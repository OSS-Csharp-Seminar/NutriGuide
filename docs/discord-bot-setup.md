# Discord Bot Setup

This guide explains how to set up and test the NutriGuide Discord bot locally.

The bot runs inside `NutriGuide.Web`. When the ASP.NET app starts, the Discord bot starts too.

## 1. Create a Discord application

1. Go to [Discord Developer Portal](https://discord.com/developers/applications).
2. Click **New Application**.
3. Name it, for example `NutriGuide Bot`.
4. Open the application.

## 2. Copy the Bot Token

1. In the left sidebar, click **Bot**.
2. Click **Add Bot** if Discord asks.
3. In the **Token** section, click **Reset Token**.
4. Copy the token from the **Bot** page.

Do not copy these values:

```text
OAuth2 Client Secret
OAuth2 Client ID
Application ID
Public Key
```

Only `Bot > Token` is valid for `DiscordBot:Token`.

If any secret was pasted into chat, screenshots, Git, or Discord, reset it immediately.

## 3. Save local user secrets

From the repo root:

```powershell
dotnet user-secrets set "DiscordBot:Token" "PASTE_BOT_TOKEN_HERE" --project NutriGuide.Web
```

Set the Groq key too, because bot commands reuse the existing AI flow:

```powershell
dotnet user-secrets set "GroqSettings:ApiKey" "PASTE_GROQ_KEY_HERE" --project NutriGuide.Web
```

## 4. Set the test server ID

Using a guild/server ID makes Discord slash commands appear quickly during development.

1. Open Discord.
2. Go to **User Settings > Advanced**.
3. Turn **Developer Mode** on.
4. Right-click your test server.
5. Click **Copy Server ID**.

Save it:

```powershell
dotnet user-secrets set "DiscordBot:GuildId" "PASTE_SERVER_ID_HERE" --project NutriGuide.Web
```

Optional check:

```powershell
dotnet user-secrets list --project NutriGuide.Web
```

Expected keys:

```text
DiscordBot:Token = ...
DiscordBot:GuildId = ...
GroqSettings:ApiKey = ...
```

## 5. Invite the bot to Discord

1. Go back to the Discord Developer Portal.
2. Open your application.
3. Go to **OAuth2 > URL Generator**.
4. Ignore the **Redirects** section. Redirect URI is not needed for this bot.
5. Under **Scopes**, select:

```text
bot
applications.commands
```

6. Under **Bot Permissions**, select:

```text
Send Messages
Use Slash Commands
```

If `Use Slash Commands` is not visible, that is fine. The important scope is `applications.commands`.

7. Scroll to the bottom.
8. Copy the **Generated URL**.
9. Open it in your browser.
10. Select your test server.
11. Click **Authorize**.

## 6. Run NutriGuide

Start the database:

```powershell
docker compose up -d
```

Apply migrations:

```powershell
dotnet ef database update --project NutriGuide.Infrastructure --startup-project NutriGuide.Web
```

Run the app:

```powershell
dotnet run --project NutriGuide.Web
```

When the app starts, the Discord bot starts inside `NutriGuide.Web`.

## 7. Link a NutriGuide user to Discord

Each developer or tester must link their own Discord account to their own NutriGuide account.

1. Log in to NutriGuide.
2. Go to **Profile**.
3. Find the **Discord bot** section.
4. Click **Generate Discord link command**.
5. Click **Copy**.
6. Paste the command in Discord.

Example:

```text
/link code:123456
```

Expected reply:

```text
Your Discord account is now linked to NutriGuide.
```

The link command expires after 10 minutes. Generate a new one if it expires.

## 8. Test commands

Log a meal:

```text
/meal text:I ate chicken and rice
```

Expected behavior:

- The bot calls the existing meal logging service.
- AI estimates nutrition.
- The meal is saved in NutriGuide.
- The source is stored as `Messaging`.
- The bot replies with calories, macros, fiber, and an AI note.

Log how you feel:

```text
/feeling text:I feel tired and bloated
```

Expected behavior:

- The bot calls the existing wellness service.
- AI checks recent nutrition context.
- A wellness log is saved.
- The bot replies with analysis and a suggested meal.

## Troubleshooting

### `401 Unauthorized`

`DiscordBot:Token` is wrong.

Use the token from **Bot > Token**, not **OAuth2 > Client Secret**.

After changing the token, fully stop and restart `NutriGuide.Web`.

### Commands do not appear

Check:

- `DiscordBot:GuildId` is set to the correct server ID.
- The app was restarted after setting secrets.
- The bot was invited with the `applications.commands` scope.

### Bot is in the server but does not respond

Make sure `NutriGuide.Web` is running. The bot only runs while the ASP.NET app is running.

### Bot says the account is not linked

Go to **Profile**, generate a Discord link command, copy it, and run it in Discord.

### AI or Groq error

Set `GroqSettings:ApiKey` in user secrets and restart the app.

## Security notes

Never commit:

```text
Discord bot token
Groq API key
OAuth2 client secret
```

If a secret is exposed, reset it immediately in the provider dashboard.
