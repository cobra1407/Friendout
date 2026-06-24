import { useState } from "react"
import { Check, ChevronDown } from "lucide-react"
import { Label } from "@/components/ui/label"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { Spinner } from "@/components/ui/spinner"
import { cn } from "@/lib/utils"
import { getTranslation } from "@/i18n"
import type { SupportedLocale } from "@/features/preferences/types/preferences.type"

const LOCALE_OPTIONS: { value: SupportedLocale; labelKey: string }[] = [
    { value: "fr", labelKey: "preferences.language.fr" },
    { value: "en", labelKey: "preferences.language.en" },
]

interface LanguageFieldProps {
    locale: SupportedLocale | undefined
    isLoading: boolean
    disabled: boolean
    onChange: (locale: SupportedLocale) => void
}

/**
 * Language field used inside the profile card (one field among name/email),
 * not a standalone Card section — see PreferencesProfileSection.
 */
export const LanguageField = ({ locale, isLoading, disabled, onChange }: LanguageFieldProps) => {
    const [open, setOpen] = useState(false)

    return (
        <div className="space-y-1.5">
            <Label className="text-sm font-medium">
                {getTranslation("preferences.language.title")}
            </Label>
            {isLoading ? (
                <div className="flex h-10 items-center"><Spinner className="size-4" /></div>
            ) : (
                // Popover instead of a native Select: the trigger has a fixed width,
                // so opening/closing the options never reflows surrounding content.
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
                            <span>
                                {locale
                                    ? getTranslation(LOCALE_OPTIONS.find((o) => o.value === locale)?.labelKey ?? "")
                                    : getTranslation("preferences.language.placeholder")}
                            </span>
                            <ChevronDown className="h-4 w-4 opacity-50" />
                        </button>
                    </PopoverTrigger>
                    <PopoverContent align="start" className="w-full p-1" style={{ width: "var(--radix-popover-trigger-width)" }}>
                        {LOCALE_OPTIONS.map((option) => (
                            <button
                                key={option.value}
                                type="button"
                                onClick={() => {
                                    onChange(option.value)
                                    setOpen(false)
                                }}
                                className="flex w-full items-center justify-between rounded-sm px-2 py-1.5 text-sm hover:bg-accent transition-colors cursor-pointer text-left"
                            >
                                {getTranslation(option.labelKey)}
                                {option.value === locale && <Check className="h-4 w-4" />}
                            </button>
                        ))}
                    </PopoverContent>
                </Popover>
            )}
            <p className="text-xs text-muted-foreground">
                {getTranslation("preferences.language.description")}
            </p>
        </div>
    )
}
