import { useEffect } from "react";
import { getHubConnection } from "@/lib/signalr/hubConnection";
import type { Activity } from "@/features/activity/types/activity.type";

interface UseRealtimeActivitiesFeedOptions {
    onNewActivity: (activity: Activity) => void;
    onDeletedActivity: (activityId: string) => void;
}

/**
 * Subscribes to the "NewActivity"/"DeletedActivity" events broadcast to every connected client
 * (see ActivitiesHubNotifier on the backend — this app is single-tenant, so every authenticated
 * user is meant to see every activity, no group/guild filtering needed here).
 */
export function useRealtimeActivitiesFeed({ onNewActivity , onDeletedActivity }: UseRealtimeActivitiesFeedOptions) {
    useEffect(() => {
        const connection = getHubConnection();

        connection.on("NewActivity", onNewActivity);
        connection.on("DeletedActivity", onDeletedActivity);

        return () => {
            connection.off("NewActivity", onNewActivity);
            connection.off("DeletedActivity", onDeletedActivity);
        };
    }, [onNewActivity, onDeletedActivity]);
}
