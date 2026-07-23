using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.Activity;
using Friendout.Domain.DTOs.Comment;
using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.WebSocketHub;

/// <summary>
/// Sends events through IActivitiesHub (SignalR's IHubContext, registered as a singleton by
/// AddSignalR — safe to inject directly, unlike FriendoutDbContext, since it holds no
/// request-scoped state).
/// </summary>
public class ActivitiesHubNotifier : IActivitiesHubNotifier
{
    private readonly IHubContext<ActivitiesHub> _hub;
    private readonly ILogger<ActivitiesHubNotifier> _logger;

    public ActivitiesHubNotifier(IHubContext<ActivitiesHub> hub, ILogger<ActivitiesHubNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task NotifyNewActivityAsync(ActivityDto activity) =>
        SafeSend(() => _hub.Clients.All.SendAsync("NewActivity", activity), nameof(NotifyNewActivityAsync));

    public Task NotifyDeletedActivityAsync(string activityId) =>
        SafeSend(() => _hub.Clients.All.SendAsync("DeletedActivity", activityId), nameof(NotifyDeletedActivityAsync));


    public Task NotifyNewCommentAsync(string activityId, CommentDto comment) =>
        SafeSend(() => _hub.Clients.Group(ActivitiesHub.ActivityGroupName(activityId)).SendAsync("NewComment", comment),
            nameof(NotifyNewCommentAsync));

    public Task NotifyCommentUpdatedAsync(string activityId, CommentDto comment) =>
        SafeSend(() => _hub.Clients.Group(ActivitiesHub.ActivityGroupName(activityId)).SendAsync("CommentUpdated", comment),
            nameof(NotifyCommentUpdatedAsync));

    public Task NotifyCommentDeletedAsync(string activityId, string commentId) =>
        SafeSend(() => _hub.Clients.Group(ActivitiesHub.ActivityGroupName(activityId)).SendAsync("CommentDeleted", commentId),
            nameof(NotifyCommentDeletedAsync));


    public Task NotifyParticipantsChangedAsync(string activityId, UserActivityParticipantsDto participants) =>
        SafeSend(() => _hub.Clients.Group(ActivitiesHub.ActivityGroupName(activityId)).SendAsync("ParticipantsChanged", participants),
            nameof(NotifyParticipantsChangedAsync));

    public Task NotifyUserAsync(string userId, NotificationType type, Dictionary<string, string> data) =>
        SafeSend(() => _hub.Clients.User(userId).SendAsync("NotificationReceived", new { type = type.ToString(), data }),
            nameof(NotifyUserAsync));

    private async Task SafeSend(Func<Task> send, string eventName)
    {
        try
        {
            await send();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push real-time event {EventName}", eventName);
        }
    }
}
