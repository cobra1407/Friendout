import { useTheme } from '@/contexts/ThemeContext'
import type { BaseTheme, AccentColor } from '@/contexts/ThemeContext'
import { cn } from '@/lib/utils'

const baseThemes: { value: BaseTheme; label: string }[] = [
  { value: 'light', label: '☀️ Clair' },
  { value: 'dark',  label: '🌙 Sombre' },
]

const accentColors: { value: AccentColor; label: string; color: string }[] = [
  { value: 'default', label: 'Défaut', color: 'bg-[oklch(0.12_0.03_196)]' },
  { value: 'blue',    label: 'Bleu',   color: 'bg-blue-600' },
  { value: 'purple',  label: 'Violet', color: 'bg-purple-600' },
  { value: 'green',   label: 'Vert',   color: 'bg-green-600' },
  { value: 'orange',  label: 'Orange', color: 'bg-orange-500' },
]

export const ThemeToggle = () => {
  const { baseTheme, accentColor, setBaseTheme, setAccentColor } = useTheme()

  return (
    <div className="flex flex-col gap-3 p-3 bg-card rounded-lg border border-border">
      {/* Sélecteur clair / sombre */}
      <div className="flex items-center gap-2">
        <span className="text-xs font-medium text-muted-foreground w-16">Mode</span>
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
              {t.label}
            </button>
          ))}
        </div>
      </div>

      {/* Sélecteur de couleur d'accentuation */}
      <div className="flex items-center gap-2">
        <span className="text-xs font-medium text-muted-foreground w-16">Couleur</span>
        <div className="flex gap-1.5">
          {accentColors.map((c) => (
            <button
              key={c.value}
              onClick={() => setAccentColor(c.value)}
              title={c.label}
              className={cn(
                'w-6 h-6 rounded-full transition-all border-2 cursor-pointer',
                c.color,
                accentColor === c.value
                  ? 'border-foreground scale-110'
                  : 'border-transparent opacity-70 hover:opacity-100'
              )}
            />
          ))}
        </div>
      </div>
    </div>
  )
}
