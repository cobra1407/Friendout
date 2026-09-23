using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.User;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Handles the out-of-band (no-login-required) account deletion flow.
/// See IAccountDeletionService for the full design rationale.
/// </summary>
public class AccountDeletionService : IAccountDeletionService
{
    private readonly FriendoutDbContext _db;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IAppLogService _appLog;
    private readonly AppOptions _appOptions;
    private readonly IConfiguration _configuration;
    private const int TokenLifetimeHours = 1;

    public AccountDeletionService(
        FriendoutDbContext db,
        INotificationDispatcher notificationDispatcher,
        IAppLogService appLog,
        IOptions<AppOptions> appOptions,
        IConfiguration configuration)
    {
        _db = db;
        _notificationDispatcher = notificationDispatcher;
        _appLog = appLog;
        _appOptions = appOptions.Value;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<bool>> RequestDeletionAsync(string email, bool deleteCreatedActivities)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        // No account for this email — still return success to avoid confirming
        // to a caller whether a given address has an account (email enumeration).
        if (user is null)
        {
            await _appLog.LogInfoAsync("AccountDeletion",
                "Deletion requested for an email with no matching account.");
            return ServiceResult<bool>.Success(true);
        }

        var request = new AccountDeletionRequest
        {
            Token = GenerateSecureToken(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(TokenLifetimeHours),
            DeleteCreatedActivities = deleteCreatedActivities
        };

        _db.AccountDeletionRequests.Add(request);
        await _db.SaveChangesAsync();

        var confirmBaseUrl = _configuration["Frontend:AccountDeletionConfirmUrl"]
            ?? $"{_appOptions.Url}/account-deletion/confirm";
        var confirmUrl = $"{confirmBaseUrl}?token={Uri.EscapeDataString(request.Token)}";

        // Guid.Empty + RecipientEmail: force email delivery regardless of the user's
        // notification preferences — this is a security-sensitive confirmation link,
        // not a preference-gated notification. Same pattern used for access request emails.
        _ = _notificationDispatcher.DispatchNotificationAsync(
            Guid.Empty,
            NotificationType.AccountDeletionRequested,
            new Dictionary<string, string>
            {
                { "RecipientEmail", user.Email! },
                { "UserName", user.Name },
                { "UserEmail", user.Email! },
                { "ConfirmUrl", confirmUrl },
                { "AppUrl", _appOptions.Url },
                { "Locale", user.Preferences?.Locale ?? "en" }
            }
        );

        await _appLog.LogInfoAsync("AccountDeletion", $"Deletion requested for user {user.Id}");

        return ServiceResult<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<bool>> ConfirmDeletionAsync(string token)
    {
        var request = await _db.AccountDeletionRequests
            .FirstOrDefaultAsync(r => r.Token == token);

        if (request is null || request.ExpiresAt < DateTime.UtcNow)
            return ServiceResult<bool>.Failure("invalid_or_expired_token");

        return await DeleteUserDataAsync(request.UserId, request.DeleteCreatedActivities);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<bool>> DeleteUserDataAsync(string userId, bool deleteCreatedActivities)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return ServiceResult<bool>.Failure("not_found");

        // Purely-personal records: safe to remove entirely, nothing else references them.
        // Loaded and removed via RemoveRange (rather than ExecuteDeleteAsync) so this
        // works against every provider, including the InMemory provider used in tests,
        // which doesn't support bulk delete translation.
        _db.Accounts.RemoveRange(await _db.Accounts.Where(a => a.UserId == userId).ToListAsync());
        _db.RefreshTokens.RemoveRange(await _db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync());
        _db.AccountDeletionRequests.RemoveRange(await _db.AccountDeletionRequests.Where(r => r.UserId == userId).ToListAsync());
        _db.UserPreferences.RemoveRange(await _db.UserPreferences.Where(p => p.UserId == userId).ToListAsync());
        _db.UserNotificationPreferences.RemoveRange(await _db.UserNotificationPreferences.Where(p => p.UserId == userId).ToListAsync());
        _db.UserNotifications.RemoveRange(await _db.UserNotifications.Where(n => n.UserId == userId).ToListAsync());
        _db.UserAchievements.RemoveRange(await _db.UserAchievements.Where(a => a.UserId == userId).ToListAsync());
        _db.UserEquipment.RemoveRange(await _db.UserEquipment.Where(e => e.UserId == userId).ToListAsync());

        var equipmentLists = await _db.EquipmentLists.Where(l => l.UserId == userId).ToListAsync();
        var equipmentListIds = equipmentLists.Select(l => l.Id).ToList();
        var equipmentListItems = await _db.EquipmentListItems
            .Where(i => equipmentListIds.Contains(i.EquipmentListId))
            .ToListAsync();
        _db.EquipmentListItems.RemoveRange(equipmentListItems);
        _db.EquipmentLists.RemoveRange(equipmentLists);

        // This user's own participations in other people's activities: only affects
        // their own row, no one else's data — always safe to remove outright.
        _db.UserParticipation.RemoveRange(await _db.UserParticipation.Where(p => p.UserId == userId).ToListAsync());

        // Comments: authorship can't be reassigned without misattributing what someone
        // else wrote, so these are always removed outright (unlike activities below).
        _db.Comments.RemoveRange(await _db.Comments.Where(c => c.UserId == userId).ToListAsync());

        // Activities this user created.
        var createdActivities = await _db.Activities
            .Where(a => a.CreatedBy == userId)
            .Include(a => a.UserParticipations)
            .ToListAsync();

        foreach (var activity in createdActivities)
        {
            var hasOtherParticipants = activity.UserParticipations.Any(p => p.UserId != userId);

            if (deleteCreatedActivities || !hasOtherParticipants)
            {
                // Either the user explicitly asked for this (consented, was warned it
                // affects others), or there's genuinely no one else to preserve it for.
                _db.Activities.Remove(activity);
            }
            else
            {
                // Orphan it: keep the activity intact for other participants, but no
                // one owns/manages it anymore until a permissions system allows a
                // consenting participant to claim it.
                activity.CreatedBy = null;
            }
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        await _appLog.LogWarningAsync("AccountDeletion",
            $"Deleted user {userId} ({createdActivities.Count} created activities: " +
            $"{(deleteCreatedActivities ? "all deleted per user's choice" : "shared ones orphaned")})");

        return ServiceResult<bool>.Success(true);
    }

    /// <summary>
    /// Generates a cryptographically secure random token (256 bits, URL-safe base64).
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32); // 256 bits
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
