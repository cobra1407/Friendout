/**
 * Auto-discovers notification sounds from src/assets/sounds/ at BUILD time (Vite's
 * import.meta.glob) — adding a new sound is just dropping a well-named file in that folder and
 * rebuilding, no code change needed here.
 *
 * Naming convention: use kebab-case file names (e.g. "soft-bell.mp3") — the id is the filename
 * without extension, and the label shown in the picker is derived from it ("soft-bell" ->
 * "Soft Bell"). Pick a clean name up front: this id is what gets stored in a user's saved
 * preference (UserNotificationPreferences.NotificationSound on the backend), so renaming a file
 * later orphans anyone who had picked it — they just silently fall back to the first sound in
 * the list (see getNotificationSound below), nothing breaks, but they lose their choice.
 *
 * Files must live under src/assets/sounds/, NOT public/sounds/ — files in public/ are copied
 * as-is and are invisible to Vite's build-time tooling, so they can't be auto-discovered this
 * way (see the conversation that led here for the full explanation).
 */

const soundModules = import.meta.glob("/src/assets/sounds/*.{mp3,wav,ogg}", {
    eager: true,
    query: "?url",
    import: "default",
}) as Record<string, string>;

export interface NotificationSoundOption {
    id: string;
    label: string;
    file: string;
}

function humanize(fileStem: string): string {
    return fileStem
        .replace(/[-_]+/g, " ")
        .trim()
        .replace(/\b\w/g, (char) => char.toUpperCase());
}

export const NOTIFICATION_SOUNDS: NotificationSoundOption[] = Object.entries(soundModules)
    .map(([path, url]) => {
        const fileName = path.split("/").pop()!;
        const id = fileName.replace(/\.[^.]+$/, "");
        return { id, label: humanize(id), file: url };
    })
    .sort((a, b) => {
        // Anything containing "default" in its id comes first, everything else
        // alphabetically after it (covers file names like "default.mp3" or
        // "default-notification.mp3" alike).
        const aIsDefault = a.id.includes("default");
        const bIsDefault = b.id.includes("default");
        if (aIsDefault && !bIsDefault) return -1;
        if (bIsDefault && !aIsDefault) return 1;
        return a.label.localeCompare(b.label);
    });

export const DEFAULT_NOTIFICATION_SOUND_ID = NOTIFICATION_SOUNDS[0]?.id ?? "default";

export function getNotificationSound(id: string | undefined): NotificationSoundOption | undefined {
    return NOTIFICATION_SOUNDS.find((sound) => sound.id === id) ?? NOTIFICATION_SOUNDS[0];
}
