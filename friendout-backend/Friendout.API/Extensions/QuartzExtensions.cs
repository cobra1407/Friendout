using friendout_backend.Jobs;
using Quartz;

namespace friendout_backend.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollection AddAppQuartz(this IServiceCollection services)
    {
        services
            .AddQuartz(q =>
            {
                var reminderJobKey = new JobKey("ActivityReminder");

                q.AddJob<ActivityReminderJob>(opts => opts.WithIdentity(reminderJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(reminderJobKey)
                    .WithIdentity("ActivityReminder-trigger")
                    .WithCronSchedule("0 0 * * * ?") // every hour on the hour
                );

                var orphanCleanupJobKey = new JobKey("OrphanedActivityCleanup");

                q.AddJob<OrphanedActivityCleanupJob>(opts => opts.WithIdentity(orphanCleanupJobKey));

                q.AddTrigger(opts => opts
                    .ForJob(orphanCleanupJobKey)
                    .WithIdentity("OrphanedActivityCleanup-trigger")
                    .WithCronSchedule("0 30 3 * * ?") // once a day at 3:30 AM
                );
            })
            .AddQuartzHostedService(options =>
            {
                // wait for running jobs to complete before shutdown
                options.WaitForJobsToComplete = true;
            });

        return services;
    }
}
