import { Users } from "lucide-react"

import { cn } from "@/lib/utils"
import { getTranslation } from "@/i18n"
import { getSpotsLeftLabel } from "@/features/activity/utils/spots.utils"

interface SpotsIndicatorProps {
    /** Confirmed participants for the main activity. */
    confirmed: number
    /** Maximum participant limit (> 0). */
    max: number
    /**
     * When fully booked, simply displays "N participant(s)" (like cards without a limit)
     * instead of the "Full" badge. Used on list cards.
     */
    fullAsCount?: boolean
    className?: string
}

const TONES = {
    ok: {
        text: "text-emerald-600 dark:text-emerald-400",
        fill: "bg-emerald-500",
        pill: "bg-emerald-50 text-emerald-700 border border-emerald-200/60 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800",
    },
    warning: {
        text: "text-amber-600 dark:text-amber-400",
        fill: "bg-amber-500",
        pill: "bg-amber-50 text-amber-700 border border-amber-200/60 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800",
    },
    full: {
        text: "text-red-600 dark:text-red-400",
        fill: "bg-red-500",
        pill: "bg-red-100 text-red-700 border border-red-300 dark:bg-red-950/60 dark:text-red-300 dark:border-red-700",
    },
} as const

/**
 * Spots indicator for limited activities: "X spots left" pill,
 * "Registrations / X registered / Y spots" line, and a thin progress bar.
 * Green when there is margin, orange when filling up, soft red when full.
 */
export function SpotsIndicator({ confirmed, max, fullAsCount = false, className }: SpotsIndicatorProps) {
    const taken = Math.min(confirmed, max)
    const remaining = Math.max(max - confirmed, 0)
    const percent = Math.round((taken / max) * 100)
    const isFull = remaining === 0
    const isAlmostFull = !isFull && (remaining <= 2 || remaining / max <= 0.25)
    const tone = isFull ? TONES.full : isAlmostFull ? TONES.warning : TONES.ok

    return (
        <div className={cn("w-full space-y-1.5", className)}>
            <div className="flex items-center gap-2">
                <Users className={cn("h-4 w-4 shrink-0", isFull && fullAsCount ? "text-purple-600" : tone.text)} />
                {isFull && fullAsCount ? (
                    <span className="text-sm">
                        {taken}{" "}
                        {taken === 1 ? getTranslation("activity.participant") : getTranslation("activity.participants")}
                    </span>
                ) : (
                    <span className={cn("rounded-full px-3 py-0.5 text-sm font-semibold", tone.pill)}>
                        {getSpotsLeftLabel(confirmed, max)}
                    </span>
                )}
            </div>

            <div className="flex items-center justify-between gap-2 text-sm">
                <span className="text-muted-foreground">
                    {getTranslation("activity.registrations")}
                </span>
                <span className="tabular-nums text-foreground">
                    {getTranslation(
                        taken === 1 ? "activity.registered_of_one" : "activity.registered_of",
                        { confirmed: taken, max }
                    )}
                </span>
            </div>

            <div
                className="h-1.5 w-full overflow-hidden rounded-full bg-muted"
                role="progressbar"
                aria-valuemin={0}
                aria-valuemax={max}
                aria-valuenow={taken}
            >
                <div
                    className={cn("h-full rounded-full transition-all duration-500", tone.fill)}
                    style={{ width: `${percent}%` }}
                />
            </div>
        </div>
    )
}
