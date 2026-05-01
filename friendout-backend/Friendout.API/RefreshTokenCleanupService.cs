using Friendout.Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace friendout_backend;

/// <summary>
/// Background service that periodically cleans up expired and revoked refresh tokens.
///
/// Why this is needed:
/// Refresh tokens accumulate in the database over time — every login creates one,
/// and rotations create new ones while marking old ones as revoked.
/// Without cleanup, the table grows indefinitely.
///
/// Schedule: runs once per day. Expired and revoked tokens are deleted.
/// </summary>
public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);
    
    /// <summary>
    /// Constructs a new instance of <see cref="RefreshTokenCleanupService"/>.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger service.</param>
    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// The main execution loop of the service.
    ///
    /// This method runs the cleanup logic every <see cref="Interval"/>.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that can be used to stop the execution.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RefreshTokenCleanupService started. Running every {Interval}.", Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Runs the cleanup logic.
            await CleanupAsync(stoppingToken);

            // Waits for the next interval before running again.
            await Task.Delay(Interval, stoppingToken);
        }
    }
    
    /// <summary>
    /// Cleanup logic to be executed.
    ///
    /// This method deletes expired and revoked refresh tokens from the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to stop the execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();

            var cutoff = DateTime.UtcNow;

            // Delete tokens that are either:
            // - Expired naturally (ExpiresAt < now)
            // - Revoked (used once or logged out) — no need to keep them
            var deleted = await db.RefreshTokens
                .Where(t => t.ExpiresAt < cutoff || t.IsRevoked)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
                _logger.LogInformation(
                    "RefreshTokenCleanupService: deleted {Count} expired/revoked tokens.", deleted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "RefreshTokenCleanupService: error during cleanup.");
        }
    }
}