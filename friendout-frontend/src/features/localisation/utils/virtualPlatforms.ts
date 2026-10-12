import { getTranslation } from "@/i18n";

export const VIRTUAL_PLATFORMS = [
    { value: "discord", label: "Discord", color: "bg-indigo-500" },
    { value: "teamspeak", label: "TeamSpeak", color: "bg-blue-500" },
    { value: "steam", label: "Steam", color: "bg-gray-700" },
    { value: "battlenet", label: "Battle.net", color: "bg-blue-600" },
    { value: "origin", label: "Origin", color: "bg-orange-500" },
    { value: "uplay", label: "Ubisoft Connect", color: "bg-blue-500" },
    { value: "epic", label: "Epic Games", color: "bg-black" },
    { value: "gog", label: "GOG Galaxy", color: "bg-sky-700" },
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
