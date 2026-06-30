using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly FriendoutDbContext _db;
    private readonly IAppLogService _appLog;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SettingsService(FriendoutDbContext db, IAppLogService appLog, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _appLog = appLog;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Resolves the display name + id of the admin performing the current request, for log attribution.</summary>
    private async Task<(string id, string name)> GetActorAsync()
    {
        var ctx = _httpContextAccessor.HttpContext;

        var actorId = ctx?.Items["UserId"] as string
                      ?? ctx?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? "unknown";

        if (actorId == "unknown") return (actorId, "unknown");
        var actor = await _db.Users.FindAsync(actorId);
        return (actorId, actor?.Name ?? actorId);
    }

    public async Task<AccessSettingsDto> GetAccessSettingsAsync()
    {
        var settings = await _db.AppSettings
            .Where(s => s.Key == "discord_restricted" || s.Key == "google_restricted")
            .ToListAsync();

        var discordRestricted = settings
            .FirstOrDefault(s => s.Key == "discord_restricted")?.Value == "true";

        var googleRestricted = settings
            .FirstOrDefault(s => s.Key == "google_restricted")?.Value == "true";

        return new AccessSettingsDto(discordRestricted, googleRestricted);
    }

    public async Task<ServiceResult<AccessSettingsDto>> UpdateAccessSettingsAsync(UpdateAccessSettingsDto dto)
    {
        try
        {
            // A provider is unusable as a login method when it's restricted with an empty
            // allowlist (everyone is filtered out). Disabling Discord or Google entirely is a
            // valid choice on its own — the corresponding login button just disappears — but
            // refuse the update if it would leave *no* usable login method at all.
            var discordUnusable = dto.DiscordRestricted && !await _db.AllowedGuilds.AnyAsync();
            var googleUnusable  = dto.GoogleRestricted  && !await _db.AllowedEmails.AnyAsync();

            if (discordUnusable && googleUnusable)
                return ServiceResult<AccessSettingsDto>.Failure("no_login_method_left");

            var current = await GetAccessSettingsAsync();

            await UpsertAsync("discord_restricted", dto.DiscordRestricted ? "true" : "false");
            await UpsertAsync("google_restricted",  dto.GoogleRestricted  ? "true" : "false");

            await _db.SaveChangesAsync();

            var (actorId, actorName) = await GetActorAsync();

            if (dto.DiscordRestricted != current.DiscordRestricted)
            {
                if (dto.DiscordRestricted)
                    await _appLog.LogInfoAsync("Auth", $"{actorName} ({actorId}) enabled Discord login restriction.");
                else
                    await _appLog.LogWarningAsync("Auth", $"{actorName} ({actorId}) disabled Discord login restriction — access is now open to everyone.");
            }

            if (dto.GoogleRestricted != current.GoogleRestricted)
            {
                if (dto.GoogleRestricted)
                    await _appLog.LogInfoAsync("Auth", $"{actorName} ({actorId}) enabled Google login restriction.");
                else
                    await _appLog.LogWarningAsync("Auth", $"{actorName} ({actorId}) disabled Google login restriction — access is now open to everyone.");
            }

            return ServiceResult<AccessSettingsDto>.Success(
                new AccessSettingsDto(dto.DiscordRestricted, dto.GoogleRestricted));
        }
        catch (Exception ex)
        {
            return ServiceResult<AccessSettingsDto>.Failure($"Failed to update settings: {ex.Message}");
        }
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>Updates the setting if it exists, inserts it otherwise.</summary>
    private async Task UpsertAsync(string key, string value)
    {
        var setting = await _db.AppSettings.FindAsync(key);

        if (setting is null)
        {
            _db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }
    }
}
