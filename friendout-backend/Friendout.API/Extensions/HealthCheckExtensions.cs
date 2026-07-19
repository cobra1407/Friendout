using System.Text.Json;
using Friendout.Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace friendout_backend.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers the health checks used by /api/health.
    /// A short timeout is set on the database check so a stalled connection
    /// fails fast instead of tying up a request thread.
    /// </summary>
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", timeout: TimeSpan.FromSeconds(5));

        return services;
    }

    /// <summary>
    /// Maps the public /api/health endpoint (nginx already proxies /api/* to
    /// this backend, so this is reachable from outside without extra config).
    /// No [Authorize] here on purpose — Docker healthchecks and external
    /// uptime monitors need to reach it without a JWT.
    ///
    /// The response is intentionally terse (status per check, nothing else):
    /// this route is public, so exception messages / connection details must
    /// never leak here. The existing global rate limiter (60 req/min/IP,
    /// see Program.cs) already covers this endpoint like any other.
    /// </summary>
    public static IEndpointRouteBuilder MapAppHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponse
        });

        return endpoints;
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString()
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
