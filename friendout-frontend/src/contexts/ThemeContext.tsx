import { createContext, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'

export type BaseTheme = 'light' | 'dark'

/**
 * Accent color applied to buttons and interactive elements.
 * 'default' = native color of the theme (almost black in light theme, almost white in dark theme).
 * Other values correspond to the CSS class `.accent-{value}` on <html>.
 */
export type AccentColor = 'default' | 'blue' | 'purple' | 'green' | 'orange'

interface ThemeContextType {
    baseTheme: BaseTheme
    accentColor: AccentColor
    setBaseTheme: (theme: BaseTheme) => void
    setAccentColor: (color: AccentColor) => void
    /** Bascule entre clair et sombre. */
    toggleBaseTheme: () => void
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined)

export const useTheme = (): ThemeContextType => {
    const context = useContext(ThemeContext)
    if (!context) throw new Error('useTheme must be used within a ThemeProvider')
    return context
}


const VALID_BASE_THEMES: BaseTheme[] = ['light', 'dark']
const VALID_ACCENTS: AccentColor[] = ['default', 'blue', 'purple', 'green', 'orange']
const ACCENT_CLASSES = VALID_ACCENTS.filter((a) => a !== 'default').map((a) => `accent-${a}`)

const readStorage = <T extends string>(key: string, valid: T[], fallback: T): T => {
    try {
        const saved = localStorage.getItem(key) as T | null
        return saved && valid.includes(saved) ? saved : fallback
    } catch {
        return fallback
    }
}

const applyThemeClasses = (base: BaseTheme, accent: AccentColor): void => {
    const root = document.documentElement
    root.classList.toggle('dark', base === 'dark')

    root.classList.remove(...ACCENT_CLASSES)
    if (accent !== 'default') {
        root.classList.add(`accent-${accent}`)
    }
}

interface ThemeProviderProps {
    children: ReactNode
    defaultBaseTheme?: BaseTheme
    defaultAccentColor?: AccentColor
}

export const ThemeProvider = ({
    children,
    defaultBaseTheme = 'light',
    defaultAccentColor = 'default',
}: ThemeProviderProps) => {
    const [baseTheme, setBaseThemeState] = useState<BaseTheme>(() =>
        readStorage('theme-base', VALID_BASE_THEMES, defaultBaseTheme)
    )

    const [accentColor, setAccentColorState] = useState<AccentColor>(() =>
        readStorage('theme-accent', VALID_ACCENTS, defaultAccentColor)
    )

    useEffect(() => {
        applyThemeClasses(baseTheme, accentColor)
        localStorage.setItem('theme-base', baseTheme)
        localStorage.setItem('theme-accent', accentColor)
    }, [baseTheme, accentColor])

    const setBaseTheme = (theme: BaseTheme) => setBaseThemeState(theme)
    const setAccentColor = (color: AccentColor) => setAccentColorState(color)
    const toggleBaseTheme = () => setBaseThemeState((prev) => (prev === 'light' ? 'dark' : 'light'))

    return (
        <ThemeContext.Provider
            value={{ baseTheme, accentColor, setBaseTheme, setAccentColor, toggleBaseTheme }}
        >
            {children}
        </ThemeContext.Provider>
    )
}
