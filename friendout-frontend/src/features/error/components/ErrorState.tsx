type ErrorStateProps = {
    title: string
    description?: string
    icon?: React.ReactNode
    primaryAction?: {
        label: string
        onClick: () => void
    }
    secondaryAction?: {
        label: string
        onClick: () => void
    }
}

export const ErrorState = ({
    title,
    description,
    icon = "⚠️",
    primaryAction,
    secondaryAction,
}: ErrorStateProps) => {
    return (
        <div className="flex items-center justify-center min-h-[60vh] px-4">
            <div className="max-w-md w-full text-center space-y-4">

                <div className="flex justify-center">
                    <div className="h-16 w-16 rounded-full bg-muted flex items-center justify-center">
                        <span className="text-3xl">{icon}</span>
                    </div>
                </div>

                <h2 className="text-xl font-semibold">
                    {title}
                </h2>

                {description && (
                    <p className="text-muted-foreground">
                        {description}
                    </p>
                )}

                {(primaryAction || secondaryAction) && (
                    <div className="flex justify-center gap-3 pt-2">
                        {secondaryAction && (
                            <button
                                onClick={secondaryAction.onClick}
                                className="px-4 py-2 rounded-md border text-sm hover:bg-muted cursor-pointer"
                            >
                                {secondaryAction.label}
                            </button>
                        )}

                        {primaryAction && (
                            <button
                                onClick={primaryAction.onClick}
                                className="px-4 py-2 rounded-md bg-primary text-primary-foreground text-sm hover:opacity-90 cursor-pointer"
                            >
                                {primaryAction.label}
                            </button>
                        )}
                    </div>
                )}
            </div>
        </div>
    )
}
