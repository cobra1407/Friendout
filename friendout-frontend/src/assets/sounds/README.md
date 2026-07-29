# Notification sounds

Drop a sound file here to make it available in the notification sound picker
(Preferences → Notifications) — **no code change needed**. Files are
auto-discovered at build time via Vite's `import.meta.glob()`
(see `src/features/notifications/constants/notificationSounds.ts`).

## Naming rules

- **Use kebab-case**, e.g. `soft-bell.mp3`, `marimba-trill.mp3`.
- The **file name (without extension) becomes the sound's `id`**, and is what
  gets saved in a user's preference (`UserNotificationPreferences.NotificationSound`
  on the backend). Renaming or deleting a file later orphans anyone who had
  picked it — they silently fall back to the first sound in the list, nothing
  breaks, but they lose their choice. Pick a name you're willing to keep.
- The **label shown in the picker is derived automatically** from the file
  name (dashes/underscores become spaces, each word capitalized) —
  `soft-bell.mp3` → "Soft Bell". Not translatable per-locale (see the
  limitation note in `notificationSounds.ts`).
- Any file name **containing "default"** (e.g. `default.mp3`,
  `default-notification.mp3`) is always sorted first in the list, regardless
  of alphabetical order. Only meant for exactly one "this is our default"
  sound — don't put "default" in multiple file names, or you'll just get an
  arbitrary order among them (whichever the sort's tie-break lands on, i.e.
  alphabetical among themselves).

## Supported formats

`.mp3`, `.wav`, `.ogg` — see the glob pattern in `notificationSounds.ts` if
you need to add another extension.

## Where NOT to put sound files

Not in `public/sounds/` — files under `public/` are copied as-is and are
invisible to Vite's build-time tooling, so they can't be auto-discovered this
way. They have to live here, under `src/assets/sounds/`.
