import { create } from "zustand";

const STORAGE_KEY = "friendout_locale";

export type SupportedLocale = "fr" | "en";

function detectBrowserLocale(): SupportedLocale {
    return navigator.language.startsWith("fr") ? "fr" : "en";
}

function readStoredLocale(): SupportedLocale | null {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === "fr" || stored === "en" ? stored : null;
}

interface LocaleState {
    locale: SupportedLocale;
    setLocale: (locale: SupportedLocale) => void;
}

// Source of truth for the UI language. Falls back to the browser language until
// the user's saved preference (UserPreferences.Locale) is loaded from the backend.
//
// Components don't need to subscribe to this store individually — App.tsx subscribes
// once at the root, which re-renders the whole tree on change. getTranslation() reads
// `useLocaleStore.getState().locale` synchronously, so existing call sites never change.
export const useLocaleStore = create<LocaleState>((set) => ({
    locale: readStoredLocale() ?? detectBrowserLocale(),
    setLocale: (locale) => {
        localStorage.setItem(STORAGE_KEY, locale);
        set({ locale });
    },
}));
