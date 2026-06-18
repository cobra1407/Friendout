import { CheckCheck } from "lucide-react"
import { Separator } from "@/components/ui/separator"
import { Button } from "@/components/ui/button"
import { NotificationItem } from "./NotificationItem"
import type { UserNotification } from "@/features/notifications/types/notification.type"
import { getTranslation } from "@/i18n"

interface NotificationDropdownProps {
    notifications: UserNotification[]
    isLoading: boolean
    unreadCount: number
    onMarkAsRead: (id: number) => void
    onMarkAllAsRead: () => void
    onDelete: (id: number) => void
}

export function NotificationDropdown({
    notifications,
    isLoading,
    unreadCount,
    onMarkAsRead,
    onMarkAllAsRead,
    onDelete,
}: NotificationDropdownProps) {
    return (
        <div className="flex flex-col">
            {/* Header */}
            <div className="flex items-center justify-between px-4 py-3">
                <span className="text-sm font-semibold text-foreground">
                    {getTranslation("notifications.title")}
                </span>
                {unreadCount > 0 && (
                    <Button
                        variant="ghost"
                        size="sm"
                        className="h-auto py-0.5 px-2 text-xs text-muted-foreground hover:text-foreground gap-1"
                        onClick={onMarkAllAsRead}
                    >
                        <CheckCheck className="w-3.5 h-3.5" />
                        {getTranslation("notifications.mark_all_read")}
                    </Button>
                )}
            </div>

            <Separator />

            {/* List */}
            <div className="max-h-[360px] overflow-y-auto">
                {isLoading ? (
                    <p className="text-xs text-muted-foreground text-center py-8">
                        {getTranslation("common.loading")}
                    </p>
                ) : notifications.length === 0 ? (
                    <p className="text-xs text-muted-foreground text-center py-8">
                        {getTranslation("notifications.empty")}
                    </p>
                ) : (
                    notifications.map((n) => (
                        <NotificationItem
                            key={n.id}
                            notification={n}
                            onMarkAsRead={onMarkAsRead}
                            onDelete={onDelete}
                        />
                    ))
                )}
            </div>
        </div>
    )
}
