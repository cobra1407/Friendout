import { Bell, ShieldCheck } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { Badge } from "@/components/ui/badge"
import { getTranslation } from "@/i18n"
import { PreferencesNotificationsSectionSkeleton } from "@/features/preferences/components/PreferencesNotificationsSectionSkeleton"
import { NotificationSoundField } from "@/features/preferences/components/NotificationSoundField"

interface PreferencesNotificationsSectionProps {
    emailEnabled: boolean | undefined
    inAppEnabled: boolean | undefined
    notificationSound: string | undefined
    accessRequestAlertsEnabled: boolean | undefined
    isAdmin: boolean
    isLoading: boolean
    disabled: boolean
    onChangeEmail: (value: boolean) => void
    onChangeInApp: (value: boolean) => void
    onChangeSound: (value: string) => void
    onChangeAccessRequestAlerts: (value: boolean) => void
}

export const PreferencesNotificationsSection = ({
    emailEnabled,
    inAppEnabled,
    notificationSound,
    accessRequestAlertsEnabled,
    isAdmin,
    isLoading,
    disabled,
    onChangeEmail,
    onChangeInApp,
    onChangeSound,
    onChangeAccessRequestAlerts,
}: PreferencesNotificationsSectionProps) => {
    return (
        <Card className="border shadow-sm">
            <CardHeader className="pb-3">
                <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-lg bg-amber-50 dark:bg-amber-950/40">
                        <Bell className="w-4 h-4 text-amber-600" />
                    </div>
                    <div>
                        <CardTitle className="text-base">{getTranslation("preferences.notifications.title")}</CardTitle>
                        <CardDescription className="text-xs">{getTranslation("preferences.notifications.description")}</CardDescription>
                    </div>
                </div>
            </CardHeader>
            <CardContent className="pt-0 space-y-4">
                {isLoading ? (
                    <PreferencesNotificationsSectionSkeleton />
                ) : (
                    <>
                        <div className="flex items-center justify-between gap-4 py-1">
                            <div className="space-y-0.5">
                                <Label className="text-sm font-medium">
                                    {getTranslation("preferences.notifications.email_label")}
                                </Label>
                                <p className="text-xs text-muted-foreground">
                                    {getTranslation("preferences.notifications.email_description")}
                                </p>
                            </div>
                            <Switch
                                checked={emailEnabled ?? true}
                                onCheckedChange={onChangeEmail}
                                disabled={disabled}
                            />
                        </div>

                        <div className="border-t" />

                        <div className="flex items-center justify-between gap-4 py-1">
                            <div className="space-y-0.5">
                                <Label className="text-sm font-medium">
                                    {getTranslation("preferences.notifications.in_app_label")}
                                </Label>
                                <p className="text-xs text-muted-foreground">
                                    {getTranslation("preferences.notifications.in_app_description")}
                                </p>
                            </div>
                            <Switch
                                checked={inAppEnabled ?? true}
                                onCheckedChange={onChangeInApp}
                                disabled={disabled}
                            />
                        </div>

                        <div className="border-t" />

                        <NotificationSoundField
                            soundId={notificationSound}
                            isLoading={isLoading}
                            disabled={disabled || !(inAppEnabled ?? true)}
                            onChange={onChangeSound}
                        />

                        {isAdmin && (
                            <>
                                <div className="border-t" />

                                <div className="flex items-center justify-between gap-4 py-1">
                                    <div className="space-y-0.5">
                                        <div className="flex items-center gap-2">
                                            <Label className="text-sm font-medium">
                                                {getTranslation("preferences.notifications.access_request_alerts_label")}
                                            </Label>
                                            <Badge
                                                variant="secondary"
                                                className="text-[10px] px-1.5 py-0 gap-1 bg-red-50 text-red-600 dark:bg-red-950/40 dark:text-red-400"
                                            >
                                                <ShieldCheck className="w-3 h-3" />
                                                {getTranslation("preferences.notifications.admin_badge")}
                                            </Badge>
                                        </div>
                                        <p className="text-xs text-muted-foreground">
                                            {getTranslation("preferences.notifications.access_request_alerts_description")}
                                        </p>
                                    </div>
                                    <Switch
                                        checked={accessRequestAlertsEnabled ?? false}
                                        onCheckedChange={onChangeAccessRequestAlerts}
                                        disabled={disabled}
                                    />
                                </div>
                            </>
                        )}
                    </>
                )}
            </CardContent>
        </Card>
    )
}
