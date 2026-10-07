import { getTranslation } from "@/i18n"

/**
 * "Remaining spots" label for a limited activity, shared by the list card, the participants
 * card and the public page. Ex: "3 places restantes", "1 place restante", "Complet".
 */
export const getSpotsLeftLabel = (confirmed: number, max: number): string => {
    const remaining = Math.max(max - confirmed, 0)

    if (remaining === 0) return getTranslation("activity.spots_full")

    return getTranslation(
        remaining === 1 ? "activity.spots_left_one" : "activity.spots_left",
        { remaining }
    )
}
