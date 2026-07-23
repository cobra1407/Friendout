import * as signalR from "@microsoft/signalr";

// Reuses the same base URL axios is configured with (VITE_API_BASE_URL), just stripping the
// trailing "/api" segment — the hub isn't under /api (nginx proxies /hubs/ separately, see
// nginx.conf). In dev this becomes "http://localhost:5122" (api.ts's absolute URL minus "/api");
// in prod it becomes "" (relative, same-origin, proxied by nginx alongside the SPA).
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api";
const HUB_BASE_URL = API_BASE_URL.replace(/\/api\/?$/, "");
const HUB_URL = `${HUB_BASE_URL}/hubs/activities`;

/**
 * Single shared HubConnection instance for the whole app (one WebSocket connection per client,
 * not one per feature/page — mirrors the single-Hub design on the backend). Module-level
 * singleton on purpose: React components come and go, but the underlying connection should
 * persist across navigation, only starting/stopping based on auth state (see
 * useRealtimeConnection, mounted once in App.tsx).
 */
let connection: signalR.HubConnection | null = null;

// Tracks the in-flight start() call so concurrent callers (useRealtimeConnection and any
// feature hook that needs to await the connection) share the same promise instead of each
// calling start() themselves — SignalR throws if start() is called while already connecting.
let startPromise: Promise<void> | null = null;

export function getHubConnection(): signalR.HubConnection {
    if (!connection) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                withCredentials: true,
            })
            .withAutomaticReconnect()
            .build();
    }

    return connection;
}

/** Starts the shared connection if needed. Safe to call multiple times (e.g. re-running effects). */
export function startHubConnection(): Promise<void> {
    const conn = getHubConnection();

    if (conn.state === signalR.HubConnectionState.Connected) {
        return Promise.resolve();
    }

    if (!startPromise) {
        startPromise = conn.start().finally(() => {
            startPromise = null;
        });
    }

    return startPromise;
}

/** Stops the shared connection (called on logout — see useRealtimeConnection). */
export function stopHubConnection(): Promise<void> {
    const conn = getHubConnection();
    startPromise = null;
    return conn.stop();
}

/**
 * Resolves once the shared connection reaches the Connected state. For features that mount
 * independently of useRealtimeConnection's own connect attempt (e.g. landing directly on an
 * activity detail page via a shared link, before App.tsx's effect has finished connecting) and
 * need to invoke a hub method (like JoinActivityGroup) as soon as it's safe to do so.
 *
 * Does not call start() itself — useRealtimeConnection owns the connect/disconnect lifecycle
 * tied to auth state. This only waits for whatever connect attempt is already in flight, or for
 * the next reconnection if none is.
 */
export function waitForHubConnection(): Promise<void> {
    const conn = getHubConnection();

    if (conn.state === signalR.HubConnectionState.Connected) {
        return Promise.resolve();
    }

    if (startPromise) {
        return startPromise;
    }

    // No connect in flight (e.g. this ran before useRealtimeConnection's effect committed, or
    // the connection previously dropped and is between retries) — wait for the next successful
    // (re)connection instead of resolving immediately with nothing to show for it.
    return new Promise((resolve) => {
        conn.onreconnected(() => resolve());
    });
}
