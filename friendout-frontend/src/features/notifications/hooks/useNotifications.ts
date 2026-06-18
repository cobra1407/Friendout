import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import {
    getMyNotifications,
    getUnreadCount,
    markAsRead,
    markAllAsRead,
    deleteNotification,
} from "@/features/notifications/api/notifications.api"

const NOTIFICATIONS_KEY = ["notifications"]
const UNREAD_COUNT_KEY  = ["notifications", "unread-count"]

export function useNotifications() {
    const qc = useQueryClient()

    const { data: notifications = [], isLoading } = useQuery({
        queryKey: NOTIFICATIONS_KEY,
        queryFn: () => getMyNotifications(0, 20),
    })

    const { data: unreadCount = 0 } = useQuery({
        queryKey: UNREAD_COUNT_KEY,
        queryFn: getUnreadCount,
        // Todo: replace this with websockets when the backend supports it, so we don't have to poll every 30 seconds
        // Poll every 30 seconds so the badge stays fresh without websockets
        refetchInterval: 30_000,
    })

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
