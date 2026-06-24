import { useTheme } from '@/contexts/ThemeContext'
import type { BaseTheme, AccentColor } from '@/contexts/ThemeContext'
import { cn } from '@/lib/utils'
import { getTranslation } from '@/i18n'

const baseThemes: { value: BaseTheme; emoji: string; labelKey: string }[] = [
    { value: 'light', emoji: '☀️', labelKey: 'preferences.theme.mode_light' },
    { value: 'dark', emoji: '🌙', labelKey: 'preferences.theme.mode_dark' },
]

/**
 * Available accent colors.
 * Each swatch reads var(--primary) directly from CSS,
 * so it always reflects the color actually applied in the app.
 * To change a color: edit App.css only.
 */
const accentColors: { value: AccentColor; labelKey: string }[] = [
    { value: 'default', labelKey: 'preferences.theme.color_default' },
    { value: 'blue',    labelKey: 'preferences.theme.color_blue'    },
    { value: 'purple',  labelKey: 'preferences.theme.color_purple'  },
    { value: 'green',   labelKey: 'preferences.theme.color_green'   },
    { value: 'orange',  labelKey: 'preferences.theme.color_orange'  },
];

/**
 * The 'default' accent has no accent-* CSS class, so its swatch would inherit
 * whatever --primary is currently active on the page (and change color).
 * We hardcode its value here, matching what is defined in App.css.
 */
const DEFAULT_PRIMARY: Record<'light' | 'dark', string> = {
    light: 'oklch(30.5% .04 222)',
    dark:  'oklch(0.68 0.10 196)',
}

export const ThemeToggle = () => {
    const { baseTheme, accentColor, setBaseTheme, setAccentColor } = useTheme()

    return (
        <div className="flex flex-col gap-3 p-3 bg-card rounded-lg border border-border">
            {/* Light / dark selector */}
            <div className="flex items-center gap-2">
                <span className="text-xs font-medium text-muted-foreground w-16">{getTranslation('preferences.theme.mode_label')}</span>
                <div className="flex gap-1">
                    {baseThemes.map((t) => (
                        <button
                            key={t.value}
                            onClick={() => setBaseTheme(t.value)}
                            className={cn(
                                'px-3 py-1 rounded-md text-sm font-medium transition-all cursor-pointer',
                                'hover:bg-accent hover:text-accent-foreground',
                                baseTheme === t.value
                                    ? 'bg-primary text-primary-foreground'
                                    : 'text-muted-foreground'
                            )}
                        >
                            {t.emoji} {getTranslation(t.labelKey)}
                        </button>
                    ))}
                </div>
            </div>

            {/* Accent color selector */}
            <div className="flex items-center gap-2">
                <span className="text-xs font-medium text-muted-foreground w-16">{getTranslation('preferences.theme.color_label')}</span>
                <div className="flex gap-1.5">
                    {accentColors.map((c) => (
                        <button
                            key={c.value}
                            onClick={() => setAccentColor(c.value)}
                            title={getTranslation(c.labelKey)}
                            className={cn(
                                c.value !== 'default' && `accent-${c.value}`,
                                'w-6 h-6 rounded-full transition-all border-2 cursor-pointer',
                                accentColor === c.value
                                    ? 'border-foreground scale-110'
                                    : 'border-transparent opacity-70 hover:opacity-100'
                            )}
                            style={{
                                backgroundColor: c.value === 'default'
                                    ? DEFAULT_PRIMARY[baseTheme]
                                    : 'var(--primary)',
                            }}
                        />
                    ))}
                </div>
            </div>
        </div>
    )
}
