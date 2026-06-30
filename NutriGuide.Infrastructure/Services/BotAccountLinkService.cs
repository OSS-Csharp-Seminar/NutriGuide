using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Enums;
using NutriGuide.Domain.Models;
using NutriGuide.Infrastructure.Data;

namespace NutriGuide.Infrastructure.Services;

public class BotAccountLinkService : IBotAccountLinkService
{
    private const int LinkCodeMinutes = 10;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public BotAccountLinkService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public string CreateLinkCode(string userId)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        _cache.Set(CacheKey(code), userId, TimeSpan.FromMinutes(LinkCodeMinutes));

        return code;
    }

    public async Task<string?> LinkExternalAccountAsync(MessagingPlatform platform, string externalUserId, string linkCode)
    {
        if (!_cache.TryGetValue(CacheKey(linkCode), out string? userId) || string.IsNullOrWhiteSpace(userId))
            return null;

        var existingConnection = await _context.MessagingConnections
            .FirstOrDefaultAsync(connection =>
                connection.Platform == platform &&
                connection.ExternalUserId == externalUserId);

        if (existingConnection == null)
        {
            _context.MessagingConnections.Add(new MessagingConnection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Platform = platform,
                ExternalUserId = externalUserId,
                IsActive = true,
                ConnectedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingConnection.UserId = userId;
            existingConnection.IsActive = true;
            existingConnection.ConnectedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _cache.Remove(CacheKey(linkCode));

        return userId;
    }

    public async Task<string?> GetUserIdAsync(MessagingPlatform platform, string externalUserId)
    {
        return await _context.MessagingConnections
            .Where(connection =>
                connection.Platform == platform &&
                connection.ExternalUserId == externalUserId &&
                connection.IsActive)
            .Select(connection => connection.UserId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasLinkedAccountAsync(string userId, MessagingPlatform platform)
    {
        return await _context.MessagingConnections
            .AnyAsync(connection =>
                connection.UserId == userId &&
                connection.Platform == platform &&
                connection.IsActive);
    }

    public async Task RemoveLinkedAccountAsync(string userId, MessagingPlatform platform)
    {
        var connections = await _context.MessagingConnections
            .Where(connection =>
                connection.UserId == userId &&
                connection.Platform == platform &&
                connection.IsActive)
            .ToListAsync();

        foreach (var connection in connections)
        {
            connection.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }

    private static string CacheKey(string code) => $"bot-link-code:{code.Trim()}";
}
