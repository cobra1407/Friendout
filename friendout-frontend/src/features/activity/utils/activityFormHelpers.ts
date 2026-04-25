import type { Activity } from "@/features/activity/types/activity.type"
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type"
import type { SubActivity } from "@/features/subActivity/types/subActivity.type"
import type { Localisation } from "@/features/localisation/types/localisation.type"
import { resolveMediaUrl } from "@/lib/media"
import { pickLocalisation } from "@/features/localisation/utils/localisation.utils"

/** Résout l'URL d'une image uploadée vers une URL absolue affichable. */
export const formatImageUrl = (imageSrc: string | undefined | null): string | null => {
    if (!imageSrc) return null
    const resolved = resolveMediaUrl(imageSrc)
    if (resolved && resolved !== imageSrc) return resolved
    if (
        imageSrc.startsWith("http") ||
        imageSrc.startsWith("/uploads") ||
        imageSrc.startsWith("blob:") ||
        imageSrc.startsWith("data:")
    ) return resolveMediaUrl(imageSrc) ?? imageSrc
    return resolveMediaUrl(`/uploads/activities/images/${imageSrc}`) ?? `/uploads/activities/images/${imageSrc}`
}

/** Extrait HH:mm depuis une string ISO ou déjà au format HH:mm. */
export const formatToHHmm = (value: string): string => {
    // The backend returns DateTime without a timezone suffix (e.g. "2026-04-23T18:00:00").
    // Without 'Z', browsers parse it as local time instead of UTC, causing the displayed
    // time to be off by the user's UTC offset. Appending 'Z' forces UTC interpretation.
    const normalized = value && !value.endsWith('Z') && !value.includes('+') && !value.includes('-', 10)
        ? value + 'Z'
        : value

    const parsedDate = new Date(normalized)
    if (!Number.isNaN(parsedDate.getTime())) {
        const h = parsedDate.getHours().toString().padStart(2, "0")
        const m = parsedDate.getMinutes().toString().padStart(2, "0")
        return `${h}:${m}`
    }
    const maybeTime = value.slice(0, 5)
    return /^\d{2}:\d{2}$/.test(maybeTime) ? maybeTime : ""
}

/** Normalise les sous-activités pour l'affichage dans le formulaire. */
export const normalizeSubActivitiesForForm = (subActivities: SubActivity[] | undefined): SubActivity[] => {
    if (!subActivities) return []
    return subActivities.map((sa) => ({
        ...sa,
        localisation: pickLocalisation(sa as SubActivity & { location?: Localisation | null }),
        startTime: formatToHHmm(sa.startTime),
        endTime: formatToHHmm(sa.endTime),
    }))
}

/** Extrait la localisation initiale depuis les données existantes. */
export const getInitialLocalisation = (
    initialData: Activity | ActivityDetails | undefined
): Localisation | null =>
    pickLocalisation(initialData as (Activity | ActivityDetails) & { location?: Localisation | null })

/** Extrait la liste initiale du matériel requis depuis les données existantes. */
export const getInitialRequiredEquipment = (
    initialData: Activity | ActivityDetails | undefined
): string[] => {
    if (!initialData) return []
    if ("requiredEquipments" in initialData && Array.isArray(initialData.requiredEquipments)) {
        return initialData.requiredEquipments.map((item) => item.name).filter(Boolean)
    }
    return []
}
