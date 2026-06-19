using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace friendout_backend.Jobs;

/// <summary>
/// Quartz job that sends a J-1 reminder to all participants of upcoming activities.
///
/// Schedule: every hour (cron: "0 0 * * * ?").
/// Target window: activities starting between now+23h and now+25h whose reminder has not been sent yet.
/// The 2-hour window absorbs timing drift across hourly runs without risk of double-sending,
/// since ReminderSentAt is set atomically after dispatch.
///
/// Only participants with status Participating on the main activity (SubActivityId == null)
/// are notified — same filter as ActivityModified / ActivityCanceled.
/// </summary>
[DisallowConcurrentExecution] // prevents overlap if a run takes longer than the trigger interval
public class ActivityReminderJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ActivityReminderJob> _logger;
    private readonly string _appUrl;

    private static readonly TimeSpan WindowStart = TimeSpan.FromHours(23);
    private static readonly TimeSpan WindowEnd   = TimeSpan.FromHours(25);

    public ActivityReminderJob(IServiceScopeFactory scopeFactory, ILogger<ActivityReminderJob> logger, IOptions<AppOptions> appOptions)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
        _appUrl       = appOptions.Value.Url.TrimEnd('/');
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db                = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
            var dispatcher        = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
            var appLog            = scope.ServiceProvider.GetRequiredService<IAppLogService>();

            var now         = DateTime.UtcNow;
            var windowStart = now.Add(WindowStart);
            var windowEnd   = now.Add(WindowEnd);

            var activities = await db.Activities
                .Include(a => a.UserParticipations.Where(up =>
                    up.SubActivityId == null &&
                    up.Status == ParticipationStatus.Participating))
                    .ThenInclude(up => up.User)
                .Include(a => a.Localisation)
                .Include(a => a.Creator)
                .Include(a => a.Image)
                .Where(a =>
                    a.StartAt >= windowStart &&
                    a.StartAt <  windowEnd   &&
                    a.ReminderSentAt == null)
                .ToListAsync(context.CancellationToken);

            if (activities.Count == 0)
            {
                _logger.LogInformation("ActivityReminderJob: no activities to remind.");
                return;
            }

            _logger.LogInformation(
                "ActivityReminderJob: found {Count} activities to remind.", activities.Count);

            foreach (var activity in activities)
            {
                var participants = activity.UserParticipations
                    .Where(up => up.User is not null)
                    .ToList();

                foreach (var participation in participants)
                {
                    try
                    {
                        await dispatcher.DispatchNotificationAsync(
                            Guid.Parse(participation.UserId),
                            NotificationType.ActivityReminder,
                            new Dictionary<string, string>
                            {
                                ["ActivityId"]       = activity.Id,
                                ["ActivityName"]     = activity.Title,
                                ["Date"]             = activity.StartAt.ToString("dddd d MMMM yyyy à HH:mm"),
                                ["Location"]         = activity.Localisation?.Address ?? "",
                                ["OrganizerName"]    = activity.Creator?.Name ?? "",
                                ["ActivityImageUrl"] = activity.Image?.Url ?? $"{_appUrl}/email-assets/default-activity-card.png",
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "ActivityReminderJob: failed to notify user {UserId} for activity {ActivityId}.",
                            participation.UserId, activity.Id);

                        await appLog.LogErrorAsync("Notifications",
                            $"Failed to send ActivityReminder to user {participation.UserId} for activity {activity.Id}: {ex.Message}",
                            ex);
                    }
                }

                // Mark as sent regardless of individual failures —
                // a partial send is better than spamming all participants on retry.
                activity.ReminderSentAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "ActivityReminderJob: reminders sent for {Count} activities.", activities.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "ActivityReminderJob: unexpected error.");
        }
    }
}
