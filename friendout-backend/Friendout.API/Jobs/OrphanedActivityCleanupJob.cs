using Friendout.Domain.Context;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace friendout_backend.Jobs;

/// <summary>
/// Daily cleanup of orphaned activities (CreatedBy = null, left behind when their
/// creator's account was deleted and other participants were still involved).
///
/// Orphaned activities are kept around for a 30-day grace period after their date so
/// participants can still revisit comments/photos/memories, then permanently removed —
/// otherwise they'd accumulate forever with no owner able to manage or delete them.
/// </summary>
[DisallowConcurrentExecution]
public class OrphanedActivityCleanupJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrphanedActivityCleanupJob> _logger;
    private const int GracePeriodDays = 30;

    public OrphanedActivityCleanupJob(IServiceScopeFactory scopeFactory, ILogger<OrphanedActivityCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-GracePeriodDays);

            var toDelete = await db.Activities
                .Where(a => a.CreatedBy == null && a.StartAt < cutoff)
                .ToListAsync(cancellationToken);

            if (toDelete.Count == 0)
            {
                _logger.LogInformation("OrphanedActivityCleanupJob: no orphaned activities to remove.");
                return;
            }

            db.Activities.RemoveRange(toDelete);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "OrphanedActivityCleanupJob removed {Count} orphaned activities older than {Cutoff:yyyy-MM-dd}",
                toDelete.Count, cutoff);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "OrphanedActivityCleanupJob: unexpected error.");
        }
    }
}
