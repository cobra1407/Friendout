import { getNotificationSound } from "@/features/notifications/constants/notificationSounds";

// If another notification sound already played less than this long ago, skip the new one
// instead of layering it on top — several notifications can legitimately arrive within the same
// second (e.g. multiple people joining an activity in quick succession), and playing every one
// of them would overlap into a jarring mess rather than sound like separate, audible pings.
const MIN_INTERVAL_MS = 800;
let lastPlayedAt = 0;

/**
 * Plays the given notification sound (by catalog id). Browser autoplay-block failures are
 * swallowed silently on purpose: browsers block audio autoplay until the user has interacted
 * with the page at least once, so the very first notification of a fresh session may silently
 * fail to play — that's expected behavior, not a bug. The notification itself (toast/badge)
 * still shows regardless of whether the sound played.
 *
 * Other failures (bad file path, corrupt file, unsupported format) are logged in dev instead of
 * being silently swallowed the same way — otherwise a typo'd sound file could sit broken
 * indefinitely with no visible signal during development.
 */
export function playNotificationSound(soundId: string | undefined) {
    const now = Date.now();
    if (now - lastPlayedAt < MIN_INTERVAL_MS) return;

    const sound = getNotificationSound(soundId);
    if (!sound) return; // no sound files in src/assets/sounds/ at all

    lastPlayedAt = now;

    const audio = new Audio(sound.file);
    audio.volume = 0.5;
    audio.play().catch((err) => {
        const isAutoplayBlock = err instanceof DOMException && err.name === "NotAllowedError";
        if (!isAutoplayBlock && import.meta.env.DEV) {
            console.warn(`Notification sound "${sound.id}" failed to play:`, err);
        }
    });
}
