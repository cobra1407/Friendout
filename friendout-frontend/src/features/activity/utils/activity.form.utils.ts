import { toast } from "sonner"
import { getTranslation } from "@/i18n"
import type { Localisation } from "@/features/localisation/types/localisation.type"
import type { SubActivity } from "@/features/subActivity/types/subActivity.type"
import { pickLocalisation } from "@/features/localisation/utils/localisation.utils"
import { resolveMediaUrl } from "@/lib/media"
import type { Activity } from "@/features/activity/types/activity.type"
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type"

// ─── Types ────────────────────────────────────────────────────────────────

export interface FormErrors {
    title?: string
    description?: string
    startAt?: string
    time?: string
    localisation?: string
}

// ─── Formatage ────────────────────────────────────────────────────────────

export const formatToHHmm = (value: string): string => {
    const parsedDate = new Date(value)
    if (!Number.isNaN(parsedDate.getTime())) {
        return `${parsedDate.getHours().toString().padStart(2, "0")}:${parsedDate.getMinutes().toString().padStart(2, "0")}`
    }
    const maybeTime = value.slice(0, 5)
    return /^\d{2}:\d{2}$/.test(maybeTime) ? maybeTime : ""
}

export const formatImageUrl = (imageSrc: string | undefined | null): string | null => {
    if (!imageSrc) return null
    const resolved = resolveMediaUrl(imageSrc)
    if (resolved && resolved !== imageSrc) return resolved
    if (imageSrc.startsWith("http") || imageSrc.startsWith("/uploads") || imageSrc.startsWith("blob:") || imageSrc.startsWith("data:"))
        return resolveMediaUrl(imageSrc) ?? imageSrc
    return resolveMediaUrl(`/uploads/activities/images/${imageSrc}`) ?? `/uploads/activities/images/${imageSrc}`
}

// ─── Initialisation depuis les données existantes ─────────────────────────

export const getInitialLocalisation = (
    initialData: Activity | ActivityDetails | undefined
): Localisation | null =>
    pickLocalisation(initialData as (Activity | ActivityDetails) & { location?: Localisation | null })

export const getInitialRequiredEquipment = (
    initialData: Activity | ActivityDetails | undefined
): string[] => {
    if (!initialData) return []
    if ("requiredEquipments" in initialData && Array.isArray(initialData.requiredEquipments)) {
        return initialData.requiredEquipments.map((item) => item.name).filter(Boolean)
    }
    return []
}

export const normalizeSubActivitiesForForm = (subActivities: SubActivity[] | undefined): SubActivity[] => {
    if (!subActivities) return []
    return subActivities.map((sa) => ({
        ...sa,
        localisation: pickLocalisation(sa as SubActivity & { location?: Localisation | null }),
        startTime: formatToHHmm(sa.startTime),
        endTime: formatToHHmm(sa.endTime),
    }))
}

// ─── Conversion issues Zod → FormErrors ──────────────────────────────────
//
// Règle :
//  • Champs principaux → stockés dans FormErrors, affichés sous le champ.
//  • Sous-activités    → toast (le composant SubActivityManager est externe).

export const buildErrors = (
    issues: { path: (string | number)[]; message: string }[]
): FormErrors => {
    const errors: FormErrors = {}
    const subActivityToastShown = new Set<string>()

    for (const issue of issues) {
        const [p0, p1, p2] = issue.path
        const msg = issue.message

        // ── Champs principaux ─────────────────────────────────────────────
        if (p0 === "title" && !errors.title) {
            errors.title = getTranslation(
                msg === "title_too_short"
                    ? "activity_form.toast.title_too_short"
                    : "activity_form.toast.title_required"
            )
        } else if (p0 === "description" && !errors.description) {
            errors.description = getTranslation(
                msg === "description_too_short"
                    ? "activity_form.toast.description_too_short"
                    : "activity_form.toast.description_required"
            )
        } else if (p0 === "startAt" && !errors.startAt) {
            errors.startAt = getTranslation(
                msg === "date_must_be_future"
                    ? "activity_form.toast.date_must_be_future"
                    : "activity_form.toast.date_required"
            )
        } else if (p0 === "time" && !errors.time) {
            errors.time = getTranslation(
                msg === "time_invalid_format"
                    ? "activity_form.toast.time_invalid_format"
                    : "activity_form.toast.time_required"
            )
        } else if (p0 === "localisation" && !errors.localisation) {
            errors.localisation = getTranslation(
                msg === "location_required"
                    ? "activity_form.toast.location_required"
                    : "activity_form.toast.location_incomplete"
            )

        // ── Sous-activités → toast ────────────────────────────────────────
        } else if (p0 === "subActivities" && typeof p1 === "number") {
            const position = String(p1 + 1)
            const toastKey = `${p1}.${String(p2)}`
            if (subActivityToastShown.has(toastKey)) continue
            subActivityToastShown.add(toastKey)

            if (p2 === "name") {
                toast.error(getTranslation("activity_form.toast.sub_activity_name_required", { position }))
            } else if (p2 === "startTime") {
                toast.error(getTranslation(
                    msg === "time_invalid_format"
                        ? "activity_form.toast.sub_activity_time_invalid"
                        : "activity_form.toast.sub_activity_start_required",
                    { position }
                ))
            } else if (p2 === "endTime") {
                toast.error(getTranslation(
                    msg === "end_before_start"
                        ? "activity_form.toast.sub_activity_end_before_start"
                        : msg === "time_invalid_format"
                            ? "activity_form.toast.sub_activity_time_invalid"
                            : "activity_form.toast.sub_activity_end_required",
                    { position }
                ))
            } else if (p2 === "localisation") {
                toast.error(getTranslation("activity_form.toast.sub_activity_location_incomplete", { position }))
            }
        }
    }

    return errors
}
