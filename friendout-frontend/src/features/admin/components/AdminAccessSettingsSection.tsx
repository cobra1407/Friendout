import { Shield } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { useAdminSettings } from "../hooks/useAdmin";

export const AdminAccessSettingsSection = () => {
    const { settings, isLoading, updateMutation } = useAdminSettings();

    const handleToggle = (key: "discordRestricted" | "googleRestricted") => {
        if (!settings) return;
        updateMutation.mutate({
            ...settings,
            [key]: !settings[key],
        });
    };

    return (
        <Card className="border shadow-sm">
            <CardHeader className="pb-3">
                <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-lg bg-rose-50 dark:bg-rose-950/40">
                        <Shield className="w-4 h-4 text-rose-600" />
                    </div>
                    <div>
                        <CardTitle className="text-base">{getTranslation('admin.settings.title')}</CardTitle>
                        <CardDescription className="text-xs">{getTranslation('admin.settings.description')}</CardDescription>
                    </div>
                </div>
            </CardHeader>
            <CardContent className="pt-0 space-y-4">
                {isLoading ? (
                    <div className="flex justify-center py-3"><Spinner /></div>
                ) : (
                    <>
                        {/* Discord restriction toggle */}
                        <div className="flex items-center justify-between gap-4 py-1">
                            <div className="space-y-0.5">
                                <Label className="text-sm font-medium">
                                    {getTranslation('admin.settings.discord_restricted_label')}
                                </Label>
                                <p className="text-xs text-muted-foreground">
                                    {settings?.discordRestricted
                                        ? getTranslation('admin.settings.discord_restricted_on')
                                        : getTranslation('admin.settings.discord_restricted_off')}
                                </p>
                            </div>
                            <Switch
                                checked={settings?.discordRestricted ?? false}
                                onCheckedChange={() => handleToggle("discordRestricted")}
                                disabled={updateMutation.isPending}
                            />
                        </div>

                        <div className="border-t" />

                        {/* Google restriction toggle */}
                        <div className="flex items-center justify-between gap-4 py-1">
                            <div className="space-y-0.5">
                                <Label className="text-sm font-medium">
                                    {getTranslation('admin.settings.google_restricted_label')}
                                </Label>
                                <p className="text-xs text-muted-foreground">
                                    {settings?.googleRestricted
                                        ? getTranslation('admin.settings.google_restricted_on')
                                        : getTranslation('admin.settings.google_restricted_off')}
                                </p>
                            </div>
                            <Switch
                                checked={settings?.googleRestricted ?? false}
                                onCheckedChange={() => handleToggle("googleRestricted")}
                                disabled={updateMutation.isPending}
                            />
                        </div>
                    </>
                )}
            </CardContent>
        </Card>
    );
};
