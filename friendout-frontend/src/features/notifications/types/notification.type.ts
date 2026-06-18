export interface UserNotification {
    id: number
    type: string
    payload: Record<string, string>
    isRead: boolean
    createdAt: string
}
