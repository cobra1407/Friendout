export const PreferencesProfileSectionSkeleton = () => {
    return (
        <div className="rounded-xl border bg-card overflow-hidden">
            <div className="h-28 sm:h-36 bg-muted animate-pulse" />
            <div className="px-4 sm:px-6 pb-6">
                <div className="-mt-10 sm:-mt-12 mb-4">
                    <div className="size-20 sm:size-24 rounded-full bg-muted animate-pulse ring-4 ring-card" />
                </div>
                <div className="mb-6 space-y-2">
                    <div className="h-5 w-40 rounded bg-muted animate-pulse" />
                    <div className="h-3.5 w-52 rounded bg-muted animate-pulse" />
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                        <div className="h-3.5 w-20 rounded bg-muted animate-pulse" />
                        <div className="h-9 rounded-md bg-muted animate-pulse" />
                    </div>
                    <div className="space-y-1.5">
                        <div className="h-3.5 w-16 rounded bg-muted animate-pulse" />
                        <div className="h-9 rounded-md bg-muted animate-pulse" />
                    </div>
                </div>
            </div>
        </div>
    )
}
