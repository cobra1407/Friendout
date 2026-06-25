using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace friendout_backend.Jobs;

/// <summary>
/// Quartz job that sends a reminder to all participants of upcoming activities.
///
/// Schedule: every hour (cron: "0 0 * * * ?").
/// Target window: activities starting between now and now+24h whose reminder has not been sent yet.
///
/// Using an open-ended "less than 24h away" window (rather than a strict [23h, 25h) slot) is
/// intentional: it makes the job self-healing. If a run is missed (crash, redeploy, downtime),
/// the next hourly run still catches any activity that's still upcoming and unsent. It also
/// naturally covers activities created late (e.g. less than 24h before they start), which would
/// otherwise never fall inside a narrow fixed window and would simply never get a reminder.
///
/// The lower bound (StartAt > now) excludes activities that have already started, so a late or
/// catch-up run never sends a reminder for something that's already happening or over.
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

    private static readonly TimeSpan ReminderLeadTime = TimeSpan.FromHours(24);

    public ActivityReminderJob(IServiceScopeFactory scopeFactory, ILogger<ActivityReminderJob> logger, IOptions<AppOptions> appOptions)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
        _appUrl       = appOptions.Value.Url.TrimEnd('/');
    }

    private string ToAbsoluteUrl(string? relativeOrAbsolute, string fallback)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            return fallback;

        if (relativeOrAbsolute.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            relativeOrAbsolute.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativeOrAbsolute;

        return $"{_appUrl}{relativeOrAbsolute}";
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db                = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();
            var dispatcher        = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
            var appLog            = scope.ServiceProvider.GetRequiredService<IAppLogService>();

            var now       = DateTime.UtcNow;
            var windowEnd = now.Add(ReminderLeadTime);

            var activities = await db.Activities
                .Include(a => a.UserParticipations.Where(up =>
                    up.SubActivityId == null &&
                    up.Status == ParticipationStatus.Participating))
                    .ThenInclude(up => up.User)
                        .ThenInclude(u => u.Preferences)
                .Include(a => a.Localisation)
                .Include(a => a.Creator)
                .Include(a => a.Image)
                .Where(a =>
                    a.StartAt >  now       &&
                    a.StartAt <= windowEnd &&
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
                                ["UserName"]         = participation.User.Name,
                                ["AppUrl"]           = _appUrl,
                                ["Locale"]           = participation.User.Preferences?.Locale ?? "en",
                                ["ActivityId"]       = activity.Id,
                                ["ActivityName"]     = activity.Title,
                                ["Date"]             = activity.StartAt.ToString("dddd d MMMM yyyy à HH:mm"),
                                ["Location"]         = activity.Localisation?.Address ?? "",
                                ["OrganizerName"]    = activity.Creator?.Name ?? "",
                                ["ActivityImageUrl"] = ToAbsoluteUrl(activity.Image?.Url, $"{_appUrl}/email-assets/default-activity-card.png"),
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
