using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.Activity;
using Friendout.Domain.DTOs.Comment;
using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.Enums;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Pushes real-time events to connected clients over the ActivitiesHub (SignalR).
///
/// Kept as an interface so services (ActivityService, CommentService, ParticipantService)
/// depend on this abstraction rather than the concrete Hub/IHubContext — mirrors the same
/// reasoning as INotificationStrategy: the caller shouldn't need to know how the push
/// actually happens under the hood.
///
/// This is a live, best-effort layer only. It never replaces the source of truth (the DB) —
/// clients that miss an event (disconnected, tab closed) simply see stale data until their
/// next fetch/poll. Callers should never assume delivery succeeded.
/// </summary>
public interface IActivitiesHubNotifier
{
    /// <summary>Broadcasts a newly created activity to every connected client (main activities feed).</summary>
    Task NotifyNewActivityAsync(ActivityDto activity);

    /// <summary>Notifies clients currently viewing this activity that the activity was deleted.</summary>
    Task NotifyDeletedActivityAsync(string activityId);

    /// <summary>Notifies clients currently viewing this activity that a new comment was posted.</summary>
    Task NotifyNewCommentAsync(string activityId, CommentDto comment);

    /// <summary>Notifies clients currently viewing this activity that a comment was edited.</summary>
    Task NotifyCommentUpdatedAsync(string activityId, CommentDto comment);

    /// <summary>Notifies clients currently viewing this activity that a comment was deleted.</summary>
    Task NotifyCommentDeletedAsync(string activityId, string commentId);

    /// <summary>
    /// Notifies clients currently viewing this activity that the participant list changed.
    /// </summary>
    Task NotifyParticipantsChangedAsync(string activityId, UserActivityParticipantsDto participants);

    /// <summary>Pushes a live notification to a specific user (complements the persisted in-app notification).</summary>
    Task NotifyUserAsync(string userId, NotificationType type, Dictionary<string, string> data);
}
