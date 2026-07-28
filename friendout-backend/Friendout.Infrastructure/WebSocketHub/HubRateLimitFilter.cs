using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.WebSocketHub;

/// <summary>
/// Rate-limits hub method invocations (JoinActivityGroup, LeaveActivityGroup) per connection.
///
/// The app's global HTTP rate limiter (Program.cs, 60 req/min/IP) only protects the initial
/// negotiate/handshake request — once a WebSocket connection is upgraded, subsequent hub method
/// calls travel over that persistent connection and never pass back through the ASP.NET Core
/// HTTP pipeline, so the HTTP limiter can't see them. Without this filter, a client could invoke
/// a hub method in a tight loop with no limit at all.
/// </summary>
public class HubRateLimitFilter : IHubFilter
{
    private const int MaxInvocationsPerWindow = 30;
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(10);

    private readonly ILogger<HubRateLimitFilter> _logger;
    private readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _counters = new();

    public HubRateLimitFilter(ILogger<HubRateLimitFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var connectionId = invocationContext.Context.ConnectionId;
        var now = DateTime.UtcNow;

        var isAllowed = true;
        _counters.AddOrUpdate(
            connectionId,
            _ => (1, now),
            (_, existing) =>
            {
                if (now - existing.WindowStart > Window)
                    return (1, now);

                if (existing.Count >= MaxInvocationsPerWindow)
                {
                    isAllowed = false;
                    return existing;
                }

                return (existing.Count + 1, existing.WindowStart);
            });

        if (!isAllowed)
        {
            _logger.LogWarning(
                "Hub rate limit exceeded on connection {ConnectionId} for method {Method}",
                connectionId, invocationContext.HubMethodName);
            throw new HubException("Too many requests. Please slow down.");
        }

        return await next(invocationContext);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        // Avoid leaking one dictionary entry per connection forever.
        _counters.TryRemove(context.Context.ConnectionId, out _);
        return next(context, exception);
    }
}
