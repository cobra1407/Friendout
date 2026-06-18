import api from "@/lib/api/api"
import type { UserNotification } from "@/features/notifications/types/notification.type"

export async function getMyNotifications(skip = 0, take = 20): Promise<UserNotification[]> {
    const response = await api.get<UserNotification[]>("/notifications", {
        params: { skip, take },
    })
    // payload arrives as a JSON string from the backend — parse it into an object
    return response.data.map((n) => ({
        ...n,
        payload: typeof n.payload === "string" ? JSON.parse(n.payload) : n.payload,
    }))
}

export async function getUnreadCount(): Promise<number> {
    const response = await api.get<number>("/notifications/unread-count")
    return response.data
}

export async function markAsRead(id: number): Promise<void> {
    await api.put(`/notifications/${id}/read`)
}

export async function markAllAsRead(): Promise<void> {
    await api.put("/notifications/read-all")
}

export async function deleteNotification(id: number): Promise<void> {
    await api.delete(`/notifications/${id}`)
}
