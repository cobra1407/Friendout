using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.Activity;
using Friendout.Domain.DTOs.Comment;
using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;

namespace Friendout.Test;

/// <summary>
/// No-op test double for IActivitiesHubNotifier. Real-time push has no observable effect in
/// unit tests (no SignalR clients connected), so tests that construct ActivityService,
/// CommentService, or ParticipantService directly just need something to satisfy the
/// constructor — shared here instead of duplicated per test file.
/// </summary>
public sealed class NoopActivitiesHubNotifier : IActivitiesHubNotifier
{
    public Task NotifyNewActivityAsync(ActivityDto activity) => Task.CompletedTask;

    public Task NotifyDeletedActivityAsync(string activityId) => Task.CompletedTask;

    public Task NotifyNewCommentAsync(string activityId, CommentDto comment) => Task.CompletedTask;

    public Task NotifyCommentUpdatedAsync(string activityId, CommentDto comment) => Task.CompletedTask;

    public Task NotifyCommentDeletedAsync(string activityId, string commentId) => Task.CompletedTask;

    public Task NotifyParticipantsChangedAsync(string activityId, UserActivityParticipantsDto participants) => Task.CompletedTask;

    public Task NotifyParticipantsChangedAsync(string activityId, UserActivityParticipationDto participants) => Task.CompletedTask;

    public Task NotifyUserAsync(string userId, NotificationType type, Dictionary<string, string> data) => Task.CompletedTask;
}
