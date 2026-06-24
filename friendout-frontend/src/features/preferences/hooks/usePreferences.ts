import { useEffect } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { getTranslation } from "@/i18n"
import { useLocaleStore } from "@/i18n/locale.store"
import { getMyPreferences, updateMyPreferences } from "@/features/preferences/api/preferences.api"
import type { UpdateUserPreferencesPayload } from "@/features/preferences/types/preferences.type"

const PREFERENCES_KEY = ["preferences", "me"]

export function usePreferences() {
    const qc = useQueryClient()
    const setLocale = useLocaleStore((state) => state.setLocale)

    const { data: preferences, isLoading } = useQuery({
        queryKey: PREFERENCES_KEY,
        queryFn: getMyPreferences,
    })

    // Once the server preference is known, it becomes the source of truth for the UI
    // language — overriding the browser-language fallback the locale store started with.
    useEffect(() => {
        if (preferences) {
            setLocale(preferences.locale)
        }
    }, [preferences, setLocale])

    const { mutate: savePreferences, isPending: isSaving } = useMutation({
        mutationFn: (payload: UpdateUserPreferencesPayload) => updateMyPreferences(payload),
        onSuccess: (data) => {
            qc.setQueryData(PREFERENCES_KEY, data)
            setLocale(data.locale)
        },
        onError: () => toast.error(getTranslation("preferences.toast_error")),
    })

    return { preferences, isLoading, savePreferences, isSaving }
}
