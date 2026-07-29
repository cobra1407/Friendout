using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Preferences;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Manages a user's own preferences (locale + notification channels).
///
/// Both UserPreferences and UserNotificationPreferences are one-to-one with User,
/// created lazily on first save with sensible defaults (see NotificationService for
/// the equivalent read-side fallback used when dispatching notifications).
/// </summary>
public class UserPreferencesService : IUserPreferencesService
{
    private static readonly HashSet<string> SupportedLocales = new() { "fr", "en" };

    private readonly FriendoutDbContext _db;

    public UserPreferencesService(FriendoutDbContext db)
    {
        _db = db;
    }

    public async Task<UserPreferencesDto> GetMyPreferencesAsync(string userId)
    {
        var userPrefs = await _db.UserPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        var notifPrefs = await _db.UserNotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        // No rows yet for this user → defaults, matching the model's own defaults.
        return new UserPreferencesDto(
            userPrefs?.Locale ?? "en",
            notifPrefs?.EmailEnabled ?? true,
            notifPrefs?.InAppEnabled ?? true,
            notifPrefs?.NotificationSound ?? "default"
        );
    }

    public async Task<ServiceResult<UserPreferencesDto>> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesDto dto)
    {
        if (!SupportedLocales.Contains(dto.Locale))
        {
            return ServiceResult<UserPreferencesDto>.Failure($"Unsupported locale: {dto.Locale}");
        }

        try
        {
            var userPrefs = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userPrefs is null)
            {
                userPrefs = new UserPreferences { UserId = userId };
                _db.UserPreferences.Add(userPrefs);
            }

            userPrefs.Locale = dto.Locale;
            userPrefs.UpdatedAt = DateTime.UtcNow;

            var notifPrefs = await _db.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (notifPrefs is null)
            {
                notifPrefs = new UserNotificationPreferences { UserId = userId };
                _db.UserNotificationPreferences.Add(notifPrefs);
            }

            notifPrefs.EmailEnabled = dto.EmailEnabled;
            notifPrefs.InAppEnabled = dto.InAppEnabled;
            notifPrefs.NotificationSound = string.IsNullOrWhiteSpace(dto.NotificationSound) ? "default" : dto.NotificationSound;
            notifPrefs.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return ServiceResult<UserPreferencesDto>.Success(
                new UserPreferencesDto(dto.Locale, dto.EmailEnabled, dto.InAppEnabled, notifPrefs.NotificationSound));
        }
        catch (Exception ex)
        {
            return ServiceResult<UserPreferencesDto>.Failure($"Failed to update preferences: {ex.Message}");
        }
    }
}
