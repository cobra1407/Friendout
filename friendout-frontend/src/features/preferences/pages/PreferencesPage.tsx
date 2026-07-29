import { useNavigate } from "react-router-dom"
import { ActivityLayout } from "@/features/activity/layout/activityLayout"
import { Header } from "@/components/header"
import { authApi } from "@/features/auth/api/auth.api"
import { getTranslation } from "@/i18n"
import { usePreferences } from "@/features/preferences/hooks/usePreferences"
import { useProfile } from "@/features/preferences/hooks/useProfile"
import { PreferencesProfileSection } from "@/features/preferences/components/PreferencesProfileSection"
import { PreferencesThemeSection } from "@/features/preferences/components/PreferencesThemeSection"
import { PreferencesNotificationsSection } from "@/features/preferences/components/PreferencesNotificationsSection"
import type { SupportedLocale } from "@/features/preferences/types/preferences.type"

export default function PreferencesPage() {
    const navigate = useNavigate()
    const { preferences, isLoading, savePreferences, isSaving } = usePreferences()
    const {
        profile,
        isLoading: isProfileLoading,
        saveName,
        isSavingName,
        uploadAvatar,
        isUploadingAvatar,
        resetAvatar,
        isResettingAvatar,
    } = useProfile()

    const handleLogout = async () => {
        await authApi.logout()
        navigate("/login")
    }

    const handleLocaleChange = (locale: SupportedLocale) => {
        if (!preferences) return
        savePreferences({ ...preferences, locale })
    }

    const handleEmailChange = (emailEnabled: boolean) => {
        if (!preferences) return
        savePreferences({ ...preferences, emailEnabled })
    }

    const handleInAppChange = (inAppEnabled: boolean) => {
        if (!preferences) return
        savePreferences({ ...preferences, inAppEnabled })
    }

    const handleSoundChange = (notificationSound: string) => {
        if (!preferences) return
        savePreferences({ ...preferences, notificationSound })
    }

    return (
        <ActivityLayout
            header={
                <Header
                    onCreateActivity={() => navigate("/activities/createActivity")}
                    onLogout={handleLogout}
                />
            }
        >
            <div className="max-w-7xl mx-auto w-full pb-10 space-y-6 px-4">
                <div className="pt-6">
                    <h1 className="text-xl font-bold text-foreground">{getTranslation("preferences.page_title")}</h1>
                    <p className="text-sm text-muted-foreground">{getTranslation("preferences.page_description")}</p>
                </div>

                <PreferencesProfileSection
                    profile={profile}
                    isLoading={isProfileLoading}
                    isSavingName={isSavingName}
                    isUploadingAvatar={isUploadingAvatar}
                    isResettingAvatar={isResettingAvatar}
                    onSaveName={saveName}
                    onUploadAvatar={uploadAvatar}
                    onResetAvatar={resetAvatar}
                    locale={preferences?.locale}
                    isLocaleLoading={isLoading}
                    isLocaleSaving={isSaving}
                    onChangeLocale={handleLocaleChange}
                />

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
                    <PreferencesThemeSection />

                    <PreferencesNotificationsSection
                        emailEnabled={preferences?.emailEnabled}
                        inAppEnabled={preferences?.inAppEnabled}
                        notificationSound={preferences?.notificationSound}
                        isLoading={isLoading}
                        disabled={isSaving}
                        onChangeEmail={handleEmailChange}
                        onChangeInApp={handleInAppChange}
                        onChangeSound={handleSoundChange}
                    />
                </div>
            </div>
        </ActivityLayout>
    )
}
