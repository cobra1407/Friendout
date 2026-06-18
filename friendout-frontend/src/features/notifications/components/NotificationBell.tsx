import { useState } from "react"
import { Bell } from "lucide-react"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { useNotifications } from "@/features/notifications/hooks/useNotifications"
import { NotificationDropdown } from "./NotificationDropdown"

export function NotificationBell() {
    const [open, setOpen] = useState(false)
    const { notifications, unreadCount, isLoading, markAsRead, markAllAsRead, deleteNotification } =
        useNotifications()

    const handleOpen = (value: boolean) => {
        setOpen(value)
    }

    return (
        <Popover open={open} onOpenChange={handleOpen}>
            <PopoverTrigger asChild>
                <button
                    className="relative p-3 rounded-md text-muted-foreground hover:text-foreground hover:bg-muted transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring cursor-pointer"
                    aria-label="Notifications"
                >
                    <Bell className="w-5.5 h-5.5 bell-ring" />
                    {unreadCount > 0 && (
                        <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white leading-none">
                            {unreadCount > 99 ? "99+" : unreadCount}
                        </span>
                    )}
                </button>
            </PopoverTrigger>

            <PopoverContent
                align="end"
                sideOffset={8}
                className="w-80 p-0 shadow-lg origin-top-right"
            >
                <NotificationDropdown
                    notifications={notifications}
                    isLoading={isLoading}
                    unreadCount={unreadCount}
                    onMarkAsRead={markAsRead}
                    onMarkAllAsRead={markAllAsRead}
                    onDelete={deleteNotification}
                />
            </PopoverContent>
        </Popover>
    )
}
