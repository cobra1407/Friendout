import { useCallback, useRef, useState } from "react"
import { useNavigate } from "react-router-dom"
import { toast } from "sonner"
import axios from "axios"

import { createActivity, updateActivity } from "@/features/activity/api/activity.api"
import { buildActivitySchema } from "@/features/activity/schema/createActivity.schema"
import { buildErrors } from "@/features/activity/utils/activityFormErrors"
import {
    formatImageUrl,
    formatToHHmm,
    getInitialLocalisation,
    getInitialRequiredEquipment,
    normalizeSubActivitiesForForm,
} from "@/features/activity/utils/activityFormHelpers"
import type { Activity } from "@/features/activity/types/activity.type"
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type"
import type { SubActivity } from "@/features/subActivity/types/subActivity.type"
import type { Localisation } from "@/features/localisation/types/localisation.type"
import type { FormErrors } from "@/features/activity/types/activityForm.type"
import { getTranslation } from "@/i18n"

interface UseActivityFormOptions {
    mode: "create" | "edit"
    initialData?: Activity | ActivityDetails
    onSuccess: (activity: Activity) => void
}

export function useActivityForm({ mode, initialData, onSuccess }: UseActivityFormOptions) {
    const navigate = useNavigate()
    const timeInputRef = useRef<HTMLInputElement | null>(null)

    // Refs for each field that can have a validation error.
    // Used to scroll to the first error after a failed submit.
    const fieldRefs = {
        title:        useRef<HTMLDivElement | null>(null),
        description:  useRef<HTMLDivElement | null>(null),
        startAt:      useRef<HTMLDivElement | null>(null),
        time:         useRef<HTMLDivElement | null>(null),
        localisation: useRef<HTMLDivElement | null>(null),
    }

    // ── État du formulaire ────────────────────────────────────────────────
    const [isLoading, setIsLoading] = useState(false)
    const [errors, setErrors] = useState<FormErrors>({})

    const [title, setTitle] = useState(initialData?.title ?? "")
    const [description, setDescription] = useState(initialData?.description ?? "")
    const [date, setDate] = useState<Date | undefined>(() => {
        if (!initialData?.startAt) return undefined
        // Same UTC normalization: append 'Z' if the backend omitted the timezone suffix.
        const raw = initialData.startAt
        const normalized = !raw.endsWith('Z') && !raw.includes('+') && !raw.includes('-', 10)
            ? raw + 'Z'
            : raw
        return new Date(normalized)
    })
    const [calendarOpen, setCalendarOpen] = useState(false)
    const [time, setTime] = useState(initialData?.startAt ? formatToHHmm(initialData.startAt) : "")
    const [estimatedPrice, setEstimatedPrice] = useState(
        initialData?.estimatedPrice != null ? String(initialData.estimatedPrice) : ""
    )
    const [localisationData, setLocalisationData] = useState<Localisation | null>(
        getInitialLocalisation(initialData)
    )
    const [requiredEquipment, setRequiredEquipment] = useState<string[]>(
        getInitialRequiredEquipment(initialData)
    )
    const [imageFile, setImageFile] = useState<File | null>(null)
    const [shouldRemoveImage, setShouldRemoveImage] = useState(false)
    const [image, setImage] = useState<string | null>(
        formatImageUrl(initialData?.image?.url ?? null)
    )
    const [subActivities, setSubActivities] = useState<SubActivity[]>(
        normalizeSubActivitiesForForm(initialData?.subActivities)
    )

    // ── Handlers ──────────────────────────────────────────────────────────

    /** Efface l'erreur d'un champ dès que l'utilisateur le modifie. */
    const clearError = useCallback(
        (field: keyof FormErrors) => setErrors((prev) => ({ ...prev, [field]: undefined })),
        []
    )

    const handleLocalisationChange = useCallback(
        (val: Localisation | null) => {
            setLocalisationData(val)
            setErrors((prev) => ({ ...prev, localisation: undefined }))
        },
        []
    )

    const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]
        if (!file) return

        const limitSize = 10 * 1024 * 1024
        if (file.size > limitSize) {
            toast.error(getTranslation("activity_form.toast.image_too_large", {
                size: String(limitSize / 1024 / 1024),
            }))
            return
        }
        if (!file.type.startsWith("image/")) {
            toast.error(getTranslation("activity_form.toast.image_invalid_type"))
            return
        }

        setImageFile(file)
        setShouldRemoveImage(false)
        const reader = new FileReader()
        reader.onload = (e) => {
            const result = e.target?.result
            if (typeof result === "string") setImage(result)
        }
        reader.readAsDataURL(file)
    }

    const removeImage = () => {
        setImage(null)
        setImageFile(null)
        setShouldRemoveImage(true)
    }

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault()
        setIsLoading(true)
        setErrors({})

        // Combine date + heure en un seul objet Date
        const startAt = date ? new Date(date) : undefined
        if (startAt && time) {
            const [hours, minutes] = time.split(":").map(Number)
            if (Number.isFinite(hours) && Number.isFinite(minutes)) {
                startAt.setHours(hours, minutes, 0, 0)
            }
        }

        const payload = {
            title,
            description,
            startAt: startAt ?? new Date(0), // new Date(0) capturé par Zod comme date invalide
            time,
            estimatedPrice: estimatedPrice ? parseFloat(estimatedPrice) : undefined,
            localisation: localisationData,
            activityImage: imageFile ?? undefined,
            removeImage: shouldRemoveImage,
            requiredEquipmentNames: requiredEquipment,
            subActivities,
        }

        // Validation Zod — le schéma est contextuel selon le mode
        // En mode "create", il vérifie aussi que la date est dans le futur
        const schema = buildActivitySchema(
            mode,
            initialData?.startAt ? new Date(initialData.startAt) : undefined
        )
        const result = schema.safeParse(payload)
        if (!result.success) {
            const newErrors = buildErrors(result.error.issues)
            setErrors(newErrors)

            // Scroll to the first field that has an error.
            // The order matches the visual order in the form.
            const errorOrder: (keyof typeof fieldRefs)[] = ['title', 'localisation', 'description', 'startAt', 'time']
            const firstErrorKey = errorOrder.find(key => newErrors[key as keyof FormErrors])
            if (firstErrorKey) {
                setTimeout(() => {
                    fieldRefs[firstErrorKey].current?.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center',
                    })
                }, 50)
            }

            setIsLoading(false)
            return
        }

        try {
            if (mode === "create") {
                const created = await createActivity(payload)
                toast.success(getTranslation("activity_form.toast.create_success"))
                onSuccess(created)
                navigate(`/activities/${created.id}`)
                return
            }

            if (!initialData?.id) {
                toast.error(getTranslation("activity_form.toast.edit_impossible"))
                return
            }

            const updated = await updateActivity(initialData.id, payload)
            toast.success(getTranslation("activity_form.toast.edit_success"))
            onSuccess(updated)
            navigate(`/activities/${updated.id}`)
        } catch (error: unknown) {
            console.error(error)
            if (axios.isAxiosError(error)) {
                const message =
                    typeof error.response?.data === "string"
                        ? error.response.data
                        : (error.response?.data?.errorMessage as string | undefined)
                toast.error(message || getTranslation("activity_form.toast.save_error"))
            } else {
                toast.error(getTranslation("activity_form.toast.save_error"))
            }
        } finally {
            setIsLoading(false)
        }
    }

    return {
        // État
        isLoading,
        errors,
        title, setTitle,
        description, setDescription,
        date, setDate,
        calendarOpen, setCalendarOpen,
        time, setTime,
        estimatedPrice, setEstimatedPrice,
        localisationData, setLocalisationData, handleLocalisationChange,
        requiredEquipment, setRequiredEquipment,
        image,
        subActivities, setSubActivities,
        timeInputRef,
        // Handlers
        clearError,
        handleImageUpload,
        removeImage,
        handleSubmit,
        fieldRefs,
    }
}
