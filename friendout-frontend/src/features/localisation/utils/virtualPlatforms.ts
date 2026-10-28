import { getTranslation } from "@/i18n";

export const VIRTUAL_PLATFORMS = [
    { value: "discord", label: "Discord", color: "bg-indigo-500" },
    { value: "teamspeak", label: "TeamSpeak", color: "bg-blue-500" },
    { value: "googlemeet", label: "Google Meet", color: "bg-green-600" },
    { value: "zoom", label: "Zoom", color: "bg-sky-600" },
    { value: "teams", label: "Microsoft Teams", color: "bg-violet-600" },
    { value: "webex", label: "Webex", color: "bg-emerald-600" },
    { value: "jitsi", label: "Jitsi Meet", color: "bg-slate-600" },
    { value: "other", label: null, color: "bg-gray-500" },
] as const;

export type VirtualPlatform = (typeof VIRTUAL_PLATFORMS)[number];

export const getVirtualPlatformItemLabel = (item: VirtualPlatform): string =>
    item.label ?? getTranslation("virtual_location_input.platform_other");

export const getVirtualPlatformLabel = (platform: string | undefined): string => {
    if (!platform) return getTranslation("virtual_location_input.platform_other");
    const item = VIRTUAL_PLATFORMS.find((candidate) => candidate.value === platform);
    return item ? getVirtualPlatformItemLabel(item) : platform;
};
