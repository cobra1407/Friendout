using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.Notification;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Query/mutation service for in-app notifications.
/// Consumed by NotificationController to expose endpoints to the frontend.
/// </summary>
public interface IInAppNotificationService
{
    Task<List<UserNotificationDto>> GetMyNotificationsAsync(string userId, int skip, int take);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteAsync(int id, string userId);
}
