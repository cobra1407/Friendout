import { useEffect } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import {
    getMyNotifications,
    getUnreadCount,
    markAsRead,
    markAllAsRead,
    deleteNotification,
} from "@/features/notifications/api/notifications.api"
import { getHubConnection } from "@/lib/signalr/hubConnection"
import { usePreferences } from "@/features/preferences/hooks/usePreferences"
import { playNotificationSound } from "@/lib/sound/playNotificationSound"

const NOTIFICATIONS_KEY = ["notifications"]
const UNREAD_COUNT_KEY  = ["notifications", "unread-count"]

export function useNotifications() {
    const qc = useQueryClient()
    const { preferences } = usePreferences()

    const { data: notifications = [], isLoading } = useQuery({
        queryKey: NOTIFICATIONS_KEY,
        queryFn: () => getMyNotifications(0, 20),
    })

    const { data: unreadCount = 0 } = useQuery({
        queryKey: UNREAD_COUNT_KEY,
        queryFn: getUnreadCount,
        // Kept as a fallback: the WebSocket push below (see useEffect) is what actually keeps
        // the badge live now, but polling stays as a safety net in case the connection is down
        // (e.g. the client's browser blocks WebSockets, or a network blip outlasts SignalR's
        // own automatic reconnect).
        refetchInterval: 30_000,
    })

    // Live push: invalidate both queries the instant a notification arrives, instead of
    // waiting for the next poll. See WebSocketNotificationStrategy on the backend — it fires
    // alongside the persisted in-app notification, not instead of it, so this is purely a
    // delivery-speed improvement, not a new data source.
    //
    // No need to check preferences.inAppEnabled before playing the sound: the backend only
    // pushes "NotificationReceived" at all when InAppEnabled is true (WebSocketNotificationStrategy
    // shares that same flag), so if this event fires, in-app notifications are already on.
    useEffect(() => {
        const connection = getHubConnection()

        const handleNotificationReceived = () => {
            qc.invalidateQueries({ queryKey: NOTIFICATIONS_KEY })
            qc.invalidateQueries({ queryKey: UNREAD_COUNT_KEY })
            playNotificationSound(preferences?.notificationSound)
        }

        connection.on("NotificationReceived", handleNotificationReceived)
        return () => {
            connection.off("NotificationReceived", handleNotificationReceived)
        }
    }, [qc, preferences?.notificationSound])

    const { mutate: read } = useMutation({
        mutationFn: markAsRead,
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: NOTIFICATIONS_KEY })
            qc.invalidateQueries({ queryKey: UNREAD_COUNT_KEY })
        },
    })

    const { mutate: readAll } = useMutation({
        mutationFn: markAllAsRead,
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: NOTIFICATIONS_KEY })
            qc.invalidateQueries({ queryKey: UNREAD_COUNT_KEY })
        },
    })

    const { mutate: remove } = useMutation({
        mutationFn: deleteNotification,
        onSuccess: () => {
            qc.invalidateQueries({ queryKey: NOTIFICATIONS_KEY })
            qc.invalidateQueries({ queryKey: UNREAD_COUNT_KEY })
        },
    })

    return {
        notifications,
        unreadCount,
        isLoading,
        markAsRead: read,
        markAllAsRead: readAll,
        deleteNotification: remove,
    }
}
