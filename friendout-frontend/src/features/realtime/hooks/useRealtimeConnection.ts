import { useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import { getHubConnection, startHubConnection, stopHubConnection } from "@/lib/signalr/hubConnection";
import { useAuth } from "@/features/auth/hooks/useAuth";

/**
 * Starts/stops the shared SignalR connection based on auth state. Mounted once in App.tsx —
 * not per-page — so the connection persists across navigation instead of reconnecting on every
 * route change.
 *
 * withAutomaticReconnect() (configured in hubConnection.ts) handles transient network blips on
 * its own; this hook only handles the "logged in" / "logged out" transitions, which
 * withAutomaticReconnect doesn't know about (it can't tell "connection dropped" from "user
 * logged out on purpose").
 */
export function useRealtimeConnection() {
    const { isAuthenticated } = useAuth();

    useEffect(() => {
        const connection = getHubConnection();

        if (!isAuthenticated) {
            if (connection.state !== signalR.HubConnectionState.Disconnected) {
                stopHubConnection();
            }
            return;
        }

        startHubConnection().catch((err) => {
            if (import.meta.env.DEV) {
                console.error("SignalR connection failed:", err);
            }
        });
    }, [isAuthenticated]);
}
