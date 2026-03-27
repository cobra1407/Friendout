import { toast } from "sonner"
import { getTranslation } from "@/i18n"
import type { FormErrors } from "@/features/activity/types/activityForm.type"

type ZodIssueLike = { path: (string | number)[]; message: string }

/**
 * Convertit les issues Zod en deux types de retour :
 * - FormErrors : erreurs inline affichées sous les champs principaux
 * - Toasts     : pour les erreurs de sous-activités (champs dans SubActivityManager)
 *
 * Une seule erreur est produite par champ (la première rencontrée).
 */
export const buildErrors = (issues: ZodIssueLike[]): FormErrors => {
    const errors: FormErrors = {}
    const subToastsShown = new Set<string>()

    for (const issue of issues) {
        const [p0, p1, p2] = issue.path
        const msg = issue.message

        // ── Champs principaux → erreur inline ────────────────────────────
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

        // ── Sous-activités → toast (SubActivityManager est externe) ──────
        } else if (p0 === "subActivities" && typeof p1 === "number") {
            const position = String(p1 + 1)
            const toastKey = `${p1}.${String(p2)}`
            if (subToastsShown.has(toastKey)) continue
            subToastsShown.add(toastKey)

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
