export type SupportedLocale = "fr" | "en"

export interface UserPreferences {
    locale: SupportedLocale
    emailEnabled: boolean
    inAppEnabled: boolean
}

export type UpdateUserPreferencesPayload = UserPreferences
