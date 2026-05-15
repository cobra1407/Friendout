using System;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly FriendoutDbContext _db;

    public SettingsService(FriendoutDbContext db)
    {
        _db = db;
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
            await UpsertAsync("discord_restricted", dto.DiscordRestricted ? "true" : "false");
            await UpsertAsync("google_restricted",  dto.GoogleRestricted  ? "true" : "false");

            await _db.SaveChangesAsync();

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
