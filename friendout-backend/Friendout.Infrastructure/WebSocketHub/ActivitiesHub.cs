using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.WebSocketHub;

/// <summary>
/// Single hub for the whole app (one WebSocket connection per client, not one per feature).
/// Real-time "channels" are modeled as SignalR groups, not separate hubs:
///
/// - Main activities feed + user notifications: no group needed. New activities broadcast to
///   Clients.All (this app is single-tenant/self-hosted per friend group — every authenticated
///   user is meant to see every activity, there's no per-guild scoping to filter on).
///   Notifications use Clients.User(userId), which SignalR resolves via the ClaimTypes.NameIdentifier
///   claim already present on the JWT — same claim AuthenticationExtensions.cs already reads
///   elsewhere, so no custom IUserIdProvider is needed.
/// - Activity detail page (comments, participants): clients explicitly join/leave a per-activity
///   group while that page is open, via JoinActivityGroup/LeaveActivityGroup below.
///
/// [Authorize] reuses the existing JWT Bearer scheme — the browser sends the "auth_token" cookie
/// automatically on the negotiate/WebSocket handshake (same mechanism OnMessageReceived already
/// relies on for regular HTTP requests), so no separate auth wiring is needed here.
/// </summary>
[Authorize]
public class ActivitiesHub : Hub
{
    private readonly ILogger<ActivitiesHub> _logger;

    public ActivitiesHub(ILogger<ActivitiesHub> logger)
    {
        _logger = logger;
    }

    /// <summary>Builds the SignalR group name for a given activity's real-time updates.</summary>
    public static string ActivityGroupName(string activityId) => $"activity-{activityId}";

    /// <summary>
    /// Joins the group for a specific activity's live updates. Called by the frontend when the
    /// activity detail page mounts. No extra authorization check here: any authenticated user can
    /// already fetch any activity via GET /api/activities/{id} (no ownership restriction on reads),
    /// so joining its real-time group carries no additional access.
    /// </summary>
    public async Task JoinActivityGroup(string activityId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ActivityGroupName(activityId));
    }

    /// <summary>Leaves the group for a specific activity. Called when the detail page unmounts.</summary>
    public async Task LeaveActivityGroup(string activityId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ActivityGroupName(activityId));
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("SignalR client connected: {ConnectionId}, user {UserId}", Context.ConnectionId, Context.UserIdentifier);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(System.Exception? exception)
    {
        _logger.LogDebug("SignalR client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
