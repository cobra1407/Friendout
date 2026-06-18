using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Notification;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Query and mutation service for in-app notifications.
/// Handles listing, counting unread, marking as read, and deletion.
/// Ownership checks are enforced on every write operation.
/// </summary>
public class InAppNotificationService : IInAppNotificationService
{
    private readonly FriendoutDbContext _db;
    private readonly ILogger<InAppNotificationService> _logger;

    public InAppNotificationService(FriendoutDbContext db, ILogger<InAppNotificationService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<List<UserNotificationDto>> GetMyNotificationsAsync(string userId, int skip, int take)
    {
        return await _db.UserNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(n => new UserNotificationDto
            {
                Id        = n.Id,
                Type      = n.Type,
                Payload   = n.Payload,
                IsRead    = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _db.UserNotifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int id, string userId)
    {
        var notification = await _db.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification is null)
        {
            _logger.LogWarning("MarkAsRead: notification {Id} not found for user {UserId}", id, userId);
            return;
        }

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _db.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var notification = await _db.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification is null)
        {
            _logger.LogWarning("Delete: notification {Id} not found for user {UserId}", id, userId);
            return;
        }

        _db.UserNotifications.Remove(notification);
        await _db.SaveChangesAsync();
    }
}
