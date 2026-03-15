import { useEffect, useMemo, useState } from "react";
import { MessageCircle, ExternalLink, Pencil, X } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";
import { getTranslation } from "@/i18n";

interface VirtualLocationInputProps {
    value: Localisation | null;
    onChange: (localisationData: Localisation | null) => void;
}

const VIRTUAL_PLATFORMS = [
    { value: "discord", label: "Discord", color: "bg-indigo-500" },
    { value: "teamspeak", label: "TeamSpeak", color: "bg-blue-500" },
    { value: "steam", label: "Steam", color: "bg-gray-700" },
    { value: "battlenet", label: "Battle.net", color: "bg-blue-600" },
    { value: "origin", label: "Origin", color: "bg-orange-500" },
    { value: "uplay", label: "Ubisoft Connect", color: "bg-blue-500" },
    { value: "epic", label: "Epic Games", color: "bg-black" },
    { value: "gog", label: "GOG Galaxy", color: "bg-sky-700" },
    { value: "other", label: getTranslation("virtual_location_input.platform_other"), color: "bg-gray-500" },
] as const;

const getPlatformLabel = (platform: string | undefined): string => {
    if (!platform) return getTranslation("virtual_location_input.platform_other");
    return VIRTUAL_PLATFORMS.find((item) => item.value === platform)?.label || platform;
};

export default function VirtualLocationInput({ value, onChange }: VirtualLocationInputProps) {
    const [isEditing, setIsEditing] = useState(true);
    const [platform, setPlatform] = useState("");
    const [serverName, setServerName] = useState("");
    const [serverInfo, setServerInfo] = useState("");

    useEffect(() => {
        if (!value || value.type !== LocalisationType.Virtual) {
            setIsEditing(true);
            setPlatform("");
            setServerName("");
            setServerInfo("");
            return;
        }

        setPlatform(value.platform || "");
        setServerName(value.address || "");
        setServerInfo(value.serverInfo || value.virtualUrl || "");
        setIsEditing(!(value.platform && value.address));
    }, [value]);

    const canSave = platform.length > 0 && serverName.trim().length > 0;

    const localisationPayload = useMemo<Localisation>(
        () => ({
            type: LocalisationType.Virtual,
            address: serverName.trim(),
            displayName: `${getPlatformLabel(platform)} - ${serverName.trim()}`,
            platform,
            serverInfo: serverInfo.trim() || undefined,
            virtualUrl: serverInfo.trim() || undefined,
        }),
        [platform, serverInfo, serverName],
    );

    const handleSave = () => {
        if (!canSave) return;
        onChange(localisationPayload);
        setIsEditing(false);
    };

    const handleRemove = () => {
        onChange(null);
        setIsEditing(true);
        setPlatform("");
        setServerName("");
        setServerInfo("");
    };

    if (!isEditing && canSave) {
        return (
            <Card className="border-blue-200 bg-blue-50">
                <CardHeader className="pb-3">
                    <CardTitle className="text-sm font-medium flex items-center gap-2">
                        <MessageCircle className="w-4 h-4 text-blue-600" />
                        {getTranslation("virtual_location_input.configured_title")}
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <div className="flex items-start justify-between gap-3">
                        <div className="flex items-start gap-3">
                            <div className={`w-6 h-6 rounded flex items-center justify-center ${VIRTUAL_PLATFORMS.find((item) => item.value === platform)?.color || "bg-gray-500"}`}>
                                <ExternalLink className="w-4 h-4 text-white" />
                            </div>
                            <div className="space-y-1">
                                <p className="text-sm font-medium">{getPlatformLabel(platform)}</p>
                                <p className="text-sm text-muted-foreground">{serverName}</p>
                                {serverInfo && <p className="text-xs text-muted-foreground">{serverInfo}</p>}
                            </div>
                        </div>
                        <div className="flex items-center gap-1">
                            <Button type="button" variant="ghost" size="sm" onClick={() => setIsEditing(true)} title={getTranslation("virtual_location_input.edit_title")}>
                                <Pencil className="w-4 h-4" />
                            </Button>
                            <Button type="button" variant="ghost" size="sm" onClick={handleRemove} title={getTranslation("virtual_location_input.remove_title")}>
                                <X className="w-4 h-4 text-red-600" />
                            </Button>
                        </div>
                    </div>

                    <div className="flex gap-2">
                        <Badge variant="outline" className="text-xs">
                            {getTranslation("virtual_location_input.badge_virtual")}
                        </Badge>
                        <Badge variant="secondary" className="text-xs">
                            {getPlatformLabel(platform)}
                        </Badge>
                    </div>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card className="border-blue-200 bg-blue-50">
            <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                    <MessageCircle className="w-4 h-4 text-blue-600" />
                    {getTranslation("virtual_location_input.configuration_title")}
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="space-y-2">
                    <Label htmlFor="virtual-platform">{getTranslation("virtual_location_input.platform_label")}</Label>
                    <Select value={platform} onValueChange={setPlatform}>
                        <SelectTrigger id="virtual-platform">
                            <SelectValue placeholder={getTranslation("virtual_location_input.platform_placeholder")} />
                        </SelectTrigger>
                        <SelectContent>
                            {VIRTUAL_PLATFORMS.map((item) => (
                                <SelectItem key={item.value} value={item.value}>
                                    <div className="flex items-center gap-2">
                                        <div className={`w-3 h-3 rounded ${item.color}`} />
                                        {item.label}
                                    </div>
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                <div className="space-y-2">
                    <Label htmlFor="virtual-server-name">{getTranslation("virtual_location_input.server_name_label")}</Label>
                    <Input
                        id="virtual-server-name"
                        value={serverName}
                        onChange={(event) => setServerName(event.target.value)}
                        placeholder={getTranslation("virtual_location_input.server_name_placeholder")}
                    />
                </div>

                <div className="space-y-2">
                    <Label htmlFor="virtual-server-info">{getTranslation("virtual_location_input.server_info_label")}</Label>
                    <Input
                        id="virtual-server-info"
                        value={serverInfo}
                        onChange={(event) => setServerInfo(event.target.value)}
                        placeholder={getTranslation("virtual_location_input.server_info_placeholder")}
                    />
                </div>

                <div className="flex gap-2 pt-2">
                    <Button type="button" size="sm" className="flex-1" onClick={handleSave} disabled={!canSave}>
                        {getTranslation("virtual_location_input.button_save")}
                    </Button>
                    <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => {
                            if (value?.type === LocalisationType.Virtual) {
                                setPlatform(value.platform || "");
                                setServerName(value.address || "");
                                setServerInfo(value.serverInfo || value.virtualUrl || "");
                                setIsEditing(!(value.platform && value.address));
                            } else {
                                handleRemove();
                            }
                        }}
                    >
                        {getTranslation("virtual_location_input.button_cancel")}
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
}
