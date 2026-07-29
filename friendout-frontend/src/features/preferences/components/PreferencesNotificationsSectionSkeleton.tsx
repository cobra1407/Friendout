export const PreferencesNotificationsSectionSkeleton = () => {
    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between py-1">
                <div className="space-y-1.5">
                    <div className="h-3.5 w-32 rounded bg-muted animate-pulse" />
                    <div className="h-3 w-48 rounded bg-muted animate-pulse" />
                </div>
                <div className="h-5 w-9 rounded-full bg-muted animate-pulse" />
            </div>
            <div className="border-t" />
            <div className="flex items-center justify-between py-1">
                <div className="space-y-1.5">
                    <div className="h-3.5 w-28 rounded bg-muted animate-pulse" />
                    <div className="h-3 w-44 rounded bg-muted animate-pulse" />
                </div>
                <div className="h-5 w-9 rounded-full bg-muted animate-pulse" />
            </div>
            <div className="border-t" />
            <div className="space-y-1.5">
                <div className="h-3.5 w-36 rounded bg-muted animate-pulse" />
                <div className="h-9 w-full rounded-md bg-muted animate-pulse" />
            </div>
        </div>
    )
}
