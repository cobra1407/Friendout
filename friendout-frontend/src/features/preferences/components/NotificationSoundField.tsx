import { useState } from "react"
import { Check, ChevronDown, Play } from "lucide-react"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Spinner } from "@/components/ui/spinner"
import { cn } from "@/lib/utils"
import { getTranslation } from "@/i18n"
import { NOTIFICATION_SOUNDS, getNotificationSound } from "@/features/notifications/constants/notificationSounds"
import { playNotificationSound } from "@/lib/sound/playNotificationSound"

interface NotificationSoundFieldProps {
    soundId: string | undefined
    isLoading: boolean
    disabled: boolean
    onChange: (soundId: string) => void
}

/**
 * Sound picker for notification preferences — same Popover-based pattern as LanguageField
 * plus a preview button so the
 * user can hear a sound before committing to it.
 *
 * Labels are plain strings derived from file names (see notificationSounds.ts), not i18n keys —
 * the sound catalog is auto-discovered from src/assets/sounds/ at build time, so there's no fixed
 * set of keys to translate ahead of time.
 */
export const NotificationSoundField = ({ soundId, isLoading, disabled, onChange }: NotificationSoundFieldProps) => {
    const [open, setOpen] = useState(false)
    const current = getNotificationSound(soundId)

    if (NOTIFICATION_SOUNDS.length === 0) {
        // No sound files in src/assets/sounds/ yet — nothing sensible to show or pick.
        return null
    }

    return (
        <div className="space-y-1.5">
            <Label className="text-sm font-medium">
                {getTranslation("preferences.notifications.sound_label")}
            </Label>
            {isLoading ? (
                <div className="flex h-9 items-center"><Spinner className="size-4" /></div>
            ) : (
                <div className="flex items-center gap-2">
                    <Popover open={open} onOpenChange={setOpen}>
                        <PopoverTrigger asChild>
                            <button
                                type="button"
                                disabled={disabled}
                                className={cn(
                                    "flex h-9 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm",
                                    "hover:bg-accent/50 transition-colors cursor-pointer",
                                    "disabled:cursor-not-allowed disabled:opacity-50"
                                )}
                            >
                                <span>{current?.label ?? "—"}</span>
                                <ChevronDown className="h-4 w-4 opacity-50" />
                            </button>
                        </PopoverTrigger>
                        <PopoverContent align="start" className="w-full p-1" style={{ width: "var(--radix-popover-trigger-width)" }}>
                            {NOTIFICATION_SOUNDS.map((option) => (
                                <button
                                    key={option.id}
                                    type="button"
                                    onClick={() => {
                                        onChange(option.id)
                                        setOpen(false)
                                    }}
                                    className="flex w-full items-center justify-between rounded-sm px-2 py-1.5 text-sm hover:bg-accent transition-colors cursor-pointer text-left"
                                >
                                    {option.label}
                                    {option.id === soundId && <Check className="h-4 w-4" />}
                                </button>
                            ))}
                        </PopoverContent>
                    </Popover>
                    <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        className="h-9 w-9 shrink-0"
                        disabled={disabled || !current}
                        onClick={() => current && playNotificationSound(current.id)}
                        title={getTranslation("preferences.notifications.sound_preview")}
                    >
                        <Play className="h-4 w-4" />
                    </Button>
                </div>
            )}
            <p className="text-xs text-muted-foreground">
                {getTranslation("preferences.notifications.sound_description")}
            </p>
        </div>
    )
}
