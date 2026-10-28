import { useMemo, useState } from "react";
import { MessageCircle, ExternalLink, Pencil, X, AlertCircle, Check, ChevronDown } from "lucide-react";

import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";
import { validateOptionalUrl } from "@/lib/maps";
import { openExternalUrl, toSafeHttpUrl } from "@/lib/safeUrl";
import {
    VIRTUAL_PLATFORMS,
    getVirtualPlatformItemLabel,
    getVirtualPlatformLabel,
} from "@/features/localisation/utils/virtualPlatforms";
import { getTranslation } from "@/i18n";

interface VirtualLocationInputProps {
    value: Localisation | null;
    onChange: (localisationData: Localisation | null) => void;
}

const getFieldsFromValue = (value: Localisation | null) => {
    if (!value || value.type !== LocalisationType.Virtual) {
        return { isEditing: true, platform: "", serverName: "", invitationLink: "", channelName: "" };
    }

    return {
        isEditing: !(value.platform && value.address),
        platform: value.platform || "",
        serverName: value.address || "",
        invitationLink: value.serverInfo || value.virtualUrl || "",
        channelName: value.channelName || "",
    };
};

export default function VirtualLocationInput({ value, onChange }: VirtualLocationInputProps) {
    const initialFields = getFieldsFromValue(value);
    const [isEditing, setIsEditing] = useState(initialFields.isEditing);
    const [platform, setPlatform] = useState(initialFields.platform);
    const [serverName, setServerName] = useState(initialFields.serverName);
    const [invitationLink, setInvitationLink] = useState(initialFields.invitationLink);
    const [invitationLinkError, setInvitationLinkError] = useState("");
    const [channelName, setChannelName] = useState(initialFields.channelName);
    const [platformOpen, setPlatformOpen] = useState(false);

    // Re-sync the fields when the parent hands us a different value
    const [prevValue, setPrevValue] = useState(value);
    if (value !== prevValue) {
        setPrevValue(value);
        const fields = getFieldsFromValue(value);
        setIsEditing(fields.isEditing);
        setPlatform(fields.platform);
        setServerName(fields.serverName);
        setInvitationLink(fields.invitationLink);
        setInvitationLinkError("");
        setChannelName(fields.channelName);
    }

    const invitationLinkValidation = validateOptionalUrl(invitationLink);
    const visibleInvitationLinkError = invitationLinkError || (invitationLinkValidation.isValid ? "" : invitationLinkValidation.error || "");
    const safeInvitationLink = toSafeHttpUrl(invitationLink);
    const selectedPlatform = VIRTUAL_PLATFORMS.find((item) => item.value === platform);

    const canSave = platform.length > 0 && serverName.trim().length > 0 && !visibleInvitationLinkError;

    const localisationPayload = useMemo<Localisation>(
        () => ({
            type: LocalisationType.Virtual,
            address: serverName.trim(),
            displayName: channelName.trim() || `${getVirtualPlatformLabel(platform)} - ${serverName.trim()}`,
            platform,
            serverInfo: invitationLink.trim() || undefined,
            virtualUrl: invitationLink.trim() || undefined,
            channelName: channelName.trim() || undefined,
        }),
        [platform, invitationLink, serverName, channelName],
    );

    const handleInvitationLinkChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const nextValue = event.target.value;
        setInvitationLink(nextValue);
        const validation = validateOptionalUrl(nextValue);
        setInvitationLinkError(validation.isValid ? "" : validation.error || "");
    };

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
        setInvitationLink("");
        setInvitationLinkError("");
        setChannelName("");
    };

    if (!isEditing && canSave) {
        return (
            <Card className="border-blue-500/30 bg-blue-500/10">
                <CardHeader className="pb-3">
                    <CardTitle className="text-sm font-medium flex items-center gap-2">
                        <MessageCircle className="w-4 h-4 text-blue-600 dark:text-blue-400" />
                        {getTranslation("virtual_location_input.configured_title")}
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <div className="flex items-start justify-between gap-3">
                        <div className="flex items-start gap-3">
                            <div className={`w-6 h-6 rounded flex items-center justify-center ${VIRTUAL_PLATFORMS.find((item) => item.value === platform)?.color || "bg-gray-500"}`}>
                                <ExternalLink className="w-4 h-4 text-white" />
                            </div>
                            <div className="space-y-1 min-w-0">
                                <p className="text-sm font-medium">{getVirtualPlatformLabel(platform)}</p>
                                <p className="text-sm text-muted-foreground truncate">{channelName || serverName}</p>
                                {safeInvitationLink && (
                                    <a
                                        href={safeInvitationLink}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-xs text-blue-700 dark:text-blue-400 hover:underline inline-flex items-center gap-1"
                                    >
                                        <ExternalLink className="w-3 h-3 shrink-0" />
                                        {getTranslation("virtual_location_input.invitation_link_open")}
                                    </a>
                                )}
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
                            {getVirtualPlatformLabel(platform)}
                        </Badge>
                    </div>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card className="border-blue-500/30 bg-blue-500/10">
            <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                    <MessageCircle className="w-4 h-4 text-blue-600 dark:text-blue-400" />
                    {getTranslation("virtual_location_input.configuration_title")}
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="space-y-2">
                    <Label htmlFor="virtual-platform">{getTranslation("virtual_location_input.platform_label")}</Label>
                    {/* Popover instead of a Radix Select: the Select repositions/resizes its list
                        while opening, which caused a visible glitch in this form. */}
                    <Popover open={platformOpen} onOpenChange={setPlatformOpen}>
                        <PopoverTrigger asChild>
                            <button
                                id="virtual-platform"
                                type="button"
                                role="combobox"
                                aria-expanded={platformOpen}
                                className="flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 cursor-pointer"
                            >
                                {selectedPlatform ? (
                                    <span className="flex items-center gap-2">
                                        <span className={`w-3 h-3 rounded ${selectedPlatform.color}`} />
                                        {getVirtualPlatformItemLabel(selectedPlatform)}
                                    </span>
                                ) : (
                                    <span className="text-muted-foreground">
                                        {getTranslation("virtual_location_input.platform_placeholder")}
                                    </span>
                                )}
                                <ChevronDown className="h-4 w-4 opacity-50" />
                            </button>
                        </PopoverTrigger>
                        <PopoverContent align="start" className="w-[var(--radix-popover-trigger-width)] p-1">
                            {VIRTUAL_PLATFORMS.map((item) => (
                                <button
                                    key={item.value}
                                    type="button"
                                    onClick={() => {
                                        setPlatform(item.value);
                                        setPlatformOpen(false);
                                    }}
                                    className={cn(
                                        "flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-sm text-left cursor-pointer hover:bg-accent hover:text-accent-foreground",
                                        platform === item.value && "bg-accent/50"
                                    )}
                                >
                                    <span className={`w-3 h-3 rounded ${item.color}`} />
                                    <span className="flex-1">{getVirtualPlatformItemLabel(item)}</span>
                                    {platform === item.value && <Check className="h-4 w-4" />}
                                </button>
                            ))}
                        </PopoverContent>
                    </Popover>
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
                    <Label htmlFor="virtual-channel-name">{getTranslation("virtual_location_input.channel_name_label")}</Label>
                    <Input
                        id="virtual-channel-name"
                        value={channelName}
                        onChange={(event) => setChannelName(event.target.value)}
                        placeholder={getTranslation("virtual_location_input.channel_name_placeholder")}
                    />
                </div>

                <div className="space-y-2">
                    <Label htmlFor="virtual-invitation-link">{getTranslation("virtual_location_input.invitation_link_label")}</Label>
                    <div className="flex gap-2">
                        <Input
                            id="virtual-invitation-link"
                            value={invitationLink}
                            onChange={handleInvitationLinkChange}
                            placeholder={getTranslation("virtual_location_input.invitation_link_placeholder")}
                            className="flex-1"
                        />
                        {safeInvitationLink && !visibleInvitationLinkError && (
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={() => openExternalUrl(safeInvitationLink)}
                                className="flex items-center gap-1 px-2 sm:px-3"
                                title={getTranslation("virtual_location_input.invitation_link_test_title")}
                            >
                                <ExternalLink className="w-4 h-4" />
                            </Button>
                        )}
                    </div>
                    {visibleInvitationLinkError && (
                        <Alert variant="destructive">
                            <AlertCircle className="h-4 w-4" />
                            <AlertDescription>{visibleInvitationLinkError}</AlertDescription>
                        </Alert>
                    )}
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
                                setInvitationLink(value.serverInfo || value.virtualUrl || "");
                                setInvitationLinkError("");
                                setChannelName(value.channelName || "");
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
