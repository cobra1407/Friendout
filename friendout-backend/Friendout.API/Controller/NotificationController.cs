using System.Security.Claims;
using Friendout.Domain.DTOs.Notification;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller for managing in-app notifications.
/// All endpoints require authentication — users can only access their own notifications.
/// </summary>
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IInAppNotificationService _notificationService;

    public NotificationController(IInAppNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Returns the authenticated user's notifications, newest first.
    /// </summary>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(List<UserNotificationDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        // Clamp take to avoid abusive queries
        take = Math.Clamp(take, 1, 100);

        var notifications = await _notificationService.GetMyNotificationsAsync(userId, skip, take);
        return Ok(notifications);
    }

    /// <summary>
    /// Returns the count of unread notifications for the badge.
    /// </summary>
    [HttpGet("notifications/unread-count")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    [HttpPut("notifications/{id:int}/read")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        await _notificationService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    /// <summary>
    /// Marks all notifications as read for the current user.
    /// </summary>
    [HttpPut("notifications/read-all")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Deletes a single notification.
    /// </summary>
    [HttpDelete("notifications/{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteNotification([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        await _notificationService.DeleteAsync(id, userId);
        return NoContent();
    }
}
