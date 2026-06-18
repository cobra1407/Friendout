import { Trash2 } from "lucide-react"
import { cn } from "@/lib/utils"
import type { UserNotification } from "@/features/notifications/types/notification.type"
import { formatNotification } from "@/features/notifications/utils/formatNotification"
import { getTranslation, getLocale } from "@/i18n"

interface NotificationItemProps {
    notification: UserNotification
    onMarkAsRead: (id: number) => void
    onDelete: (id: number) => void
}

export function NotificationItem({ notification, onMarkAsRead, onDelete }: NotificationItemProps) {
    const { title, message } = formatNotification(notification.type, notification.payload)
    const relativeTime = formatRelativeTime(notification.createdAt)

    return (
        <div
            className={cn(
                "flex items-start gap-3 px-4 py-3 transition-colors hover:bg-muted/50 group",
                !notification.isRead && "bg-blue-50/60 dark:bg-blue-950/20"
            )}
        >
            {/* Unread dot */}
            <div className="flex-shrink-0 mt-1.5">
                <span
                    className={cn(
                        "block w-2 h-2 rounded-full transition-colors",
                        notification.isRead ? "bg-transparent" : "bg-blue-500"
                    )}
                />
            </div>

            {/* Content */}
            <div
                className="flex-1 min-w-0 cursor-pointer"
                onClick={() => !notification.isRead && onMarkAsRead(notification.id)}
            >
                <p className={cn(
                    "text-sm leading-snug truncate",
                    !notification.isRead ? "font-semibold text-foreground" : "font-normal text-foreground"
                )}>
                    {title}
                </p>
                <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                    {message}
                </p>
                <p className="text-xs text-muted-foreground/70 mt-1">{relativeTime}</p>
            </div>

            {/* Delete button — visible on hover */}
            <button
                onClick={(e) => { e.stopPropagation(); onDelete(notification.id) }}
                title={getTranslation("notifications.delete")}
                className="flex-shrink-0 opacity-0 group-hover:opacity-100 transition-opacity text-muted-foreground hover:text-destructive p-1 rounded cursor-pointer"
            >
                <Trash2 className="w-3.5 h-3.5" />
            </button>
        </div>
    )
}

function formatRelativeTime(iso: string): string {
    const diff = Date.now() - new Date(iso).getTime()
    const minutes = Math.floor(diff / 60_000)
    const rtf = new Intl.RelativeTimeFormat(getLocale(), { numeric: "auto" })

    if (minutes < 1) return rtf.format(0, "minutes")
    if (minutes < 60) return rtf.format(-minutes, "minutes")
    const hours = Math.floor(minutes / 60)
    if (hours < 24) return rtf.format(-hours, "hours")
    const days = Math.floor(hours / 24)
    return rtf.format(-days, "days")
}
