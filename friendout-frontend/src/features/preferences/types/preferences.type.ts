export type SupportedLocale = "fr" | "en"

export interface UserPreferences {
    locale: SupportedLocale
    emailEnabled: boolean
    inAppEnabled: boolean
    notificationSound: string
    accessRequestAlertsEnabled: boolean
}

export type UpdateUserPreferencesPayload = UserPreferences
