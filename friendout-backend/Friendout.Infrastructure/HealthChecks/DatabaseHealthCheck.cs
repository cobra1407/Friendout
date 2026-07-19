using System;
using System.Threading;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Friendout.Infrastructure.HealthChecks;

/// <summary>
/// Verifies the API can open a connection to the MySQL database.
///
/// This check backs a PUBLICLY reachable endpoint (admin dashboard badge,
/// Docker healthcheck, external uptime monitors), so it deliberately never
/// surfaces the underlying exception message or connection details — only a
/// plain Healthy/Unhealthy result. Anything more specific (query errors,
/// connection string fragments, etc.) stays in the application logs, which
/// are only visible to admins via the admin panel.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FriendoutDbContext _dbContext;

    public DatabaseHealthCheck(FriendoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection succeeded.")
                : HealthCheckResult.Unhealthy("Database connection failed.");
        }
        catch (Exception)
        {
            // Swallow the real exception on purpose — see class remarks above.
            return HealthCheckResult.Unhealthy("Database connection failed.");
        }
    }
}
