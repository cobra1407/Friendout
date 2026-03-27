import { useRef, useState } from "react"
import { useNavigate } from "react-router-dom"
import { Upload, X, CalendarIcon, Clock } from "lucide-react"
import { toast } from "sonner"
import axios from "axios"
import { format } from "date-fns"
import { fr, enUS } from "date-fns/locale"

import EnhancedLocationInput from "@/components/EnhancedLocationInput"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Calendar } from "@/components/ui/calendar"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"

import { createActivity, updateActivity } from "@/features/activity/api/activity.api"
import { createActivitySchema } from "@/features/activity/schema/createActivity.schema"
import EquipmentManager from "@/features/equipment/component/EquipmentManager"
import SubActivityManager from "@/features/subActivity/component/SubActivityManager"
import type { Activity } from "@/features/activity/types/activity.type"
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type"
import type { SubActivity } from "@/features/subActivity/types/subActivity.type"
import type { Localisation } from "@/features/localisation/types/localisation.type"
import { resolveMediaUrl } from "@/lib/media"
import { pickLocalisation } from "@/features/localisation/utils/localisation.utils"
import { getTranslation, getLang } from "@/i18n"

const dateLocale = getLang() === "fr" ? fr : enUS

// ─── Types ────────────────────────────────────────────────────────────────

interface ActivityFormProps {
    mode: "create" | "edit"
    initialData?: Activity | ActivityDetails
    onBack: () => void
    onSuccess: (activity: Activity) => void
}

// errors for each field
interface FormErrors {
    title?: string
    description?: string
    startAt?: string
    time?: string
    localisation?: string
}

// ─── Composant d'affichage d'erreur inline ────────────────────────────────

function FieldError({ message }: { message?: string }) {
    if (!message) return null
    return (
        <p className="text-sm text-red-500 mt-1" role="alert">
            {message}
        </p>
    )
}

// ─── Utilitaires ──────────────────────────────────────────────────────────

const formatImageUrl = (imageSrc: string | undefined | null): string | null => {
    if (!imageSrc) return null
    const resolved = resolveMediaUrl(imageSrc)
    if (resolved && resolved !== imageSrc) return resolved
    if (imageSrc.startsWith("http") || imageSrc.startsWith("/uploads") || imageSrc.startsWith("blob:") || imageSrc.startsWith("data:"))
        return resolveMediaUrl(imageSrc) ?? imageSrc
    const legacyPath = `/uploads/activities/images/${imageSrc}`
    return resolveMediaUrl(legacyPath) ?? legacyPath
}

const formatToHHmm = (value: string): string => {
    const parsedDate = new Date(value)
    if (!Number.isNaN(parsedDate.getTime())) {
        return `${parsedDate.getHours().toString().padStart(2, "0")}:${parsedDate.getMinutes().toString().padStart(2, "0")}`
    }
    const maybeTime = value.slice(0, 5)
    return /^\d{2}:\d{2}$/.test(maybeTime) ? maybeTime : ""
}

const normalizeSubActivitiesForForm = (subActivities: SubActivity[] | undefined): SubActivity[] => {
    if (!subActivities) return []
    return subActivities.map((sa) => ({
        ...sa,
        localisation: pickLocalisation(sa as SubActivity & { location?: Localisation | null }),
        startTime: formatToHHmm(sa.startTime),
        endTime: formatToHHmm(sa.endTime),
    }))
}

const getInitialLocalisation = (initialData: Activity | ActivityDetails | undefined): Localisation | null =>
    pickLocalisation(initialData as (Activity | ActivityDetails) & { location?: Localisation | null })

const getInitialRequiredEquipment = (initialData: Activity | ActivityDetails | undefined): string[] => {
    if (!initialData) return []
    if ("requiredEquipments" in initialData && Array.isArray(initialData.requiredEquipments)) {
        return initialData.requiredEquipments.map((item) => item.name).filter(Boolean)
    }
    return []
}


/**
 * Builds an object containing errors for each form field from the given Zod issues.
 * Used to display errors to the user when they submit an invalid form.
 * @param issues - The issues returned by Zod's safeParse method.
 * @returns An object containing errors for each form field.
 */
const buildErrors = (
    issues: ReturnType<typeof createActivitySchema.safeParse> extends { success: false; error: infer E }
        ? E extends { issues: infer I }
        ? I
        : never
        : never
): FormErrors => {
    const errors: FormErrors = {}
    const subActivityToastShown = new Set<string>()

    for (const issue of issues as { path: (string | number)[]; message: string }[]) {
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
            errors.startAt = getTranslation("activity_form.toast.date_required")
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

/**
 * A form to create or edit an activity.
 * The form contains fields for the activity title, description, start date and time, estimated price, localisation, required equipment, and image.
 * The form also includes a list of sub-activities.
 * The form is validated using Zod.
 * When the form is submitted, it sends a POST request to the API to create or edit an activity.
 * The form is rendered as a card with a title, description, and form fields.
 * The form is intended to be used in a modal.
 * @param {ActivityFormProps} props - The props for the ActivityForm component.
 * @prop {string} mode - The mode of the form, either "create" or "edit".
 * @prop {Activity | ActivityDetails | undefined} initialData - The initial data for the form, either an Activity or ActivityDetails object.
 * @prop {() => void} onBack - The function to call when the user clicks the back button.
 * @prop {(activity: Activity) => void} onSuccess - The function to call when the form is submitted successfully.
 */
export default function ActivityForm({ mode, initialData, onBack, onSuccess }: ActivityFormProps) {
    const navigate = useNavigate()
    const timeInputRef = useRef<HTMLInputElement | null>(null)

    const [isLoading, setIsLoading] = useState(false)
    const [errors, setErrors] = useState<FormErrors>({})

    const [title, setTitle] = useState(initialData?.title ?? "")
    const [description, setDescription] = useState(initialData?.description ?? "")
    const [date, setDate] = useState<Date | undefined>(
        initialData?.startAt ? new Date(initialData.startAt) : undefined
    )
    const [calendarOpen, setCalendarOpen] = useState(false)
    const [time, setTime] = useState(initialData?.startAt ? formatToHHmm(initialData.startAt) : "")
    const [estimatedPrice, setEstimatedPrice] = useState(
        initialData?.estimatedPrice != null ? String(initialData.estimatedPrice) : ""
    )
    const [localisationData, setLocalisationData] = useState<Localisation | null>(getInitialLocalisation(initialData))
    const [requiredEquipment, setRequiredEquipment] = useState<string[]>(getInitialRequiredEquipment(initialData))
    const [imageFile, setImageFile] = useState<File | null>(null)
    const [image, setImage] = useState<string | null>(formatImageUrl(initialData?.image?.url ?? null))
    const [subActivities, setSubActivities] = useState<SubActivity[]>(
        normalizeSubActivitiesForForm(initialData?.subActivities)
    )


    /**
     * Clears an error for a specific field.
     * @param {keyof FormErrors} field The field to clear the error for.
     */
    const clearError = (field: keyof FormErrors) =>
        setErrors((prev) => ({ ...prev, [field]: undefined }))

    const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]
        if (!file) return
        const limitSize = 10 * 1024 * 1024
        if (file.size > limitSize) {
            toast.error(getTranslation("activity_form.toast.image_too_large", { size: String(limitSize / 1024 / 1024) }))
            return
        }
        if (!file.type.startsWith("image/")) {
            toast.error(getTranslation("activity_form.toast.image_invalid_type"))
            return
        }
        setImageFile(file)
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
    }

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault()
        setIsLoading(true)
        setErrors({}) // reset errors after each submit

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
            startAt: startAt ?? new Date(0),
            time,
            estimatedPrice: estimatedPrice ? parseFloat(estimatedPrice) : undefined,
            localisation: localisationData,
            activityImage: imageFile ?? undefined,
            requiredEquipmentNames: requiredEquipment,
            subActivities,
        }

        // Validation Zod
        const result = createActivitySchema.safeParse(payload)
        if (!result.success) {
            const fieldErrors = buildErrors(result.error.issues as never)
            setErrors(fieldErrors)
            setIsLoading(false)
            return
        }

        // validation date future
        if (mode === "create" && startAt && startAt <= new Date()) {
            setErrors({ startAt: getTranslation("activity_form.toast.date_must_be_future") })
            setIsLoading(false)
            return
        }

        try {
            if (mode === "create") {
                const createdActivity = await createActivity(payload)
                toast.success(getTranslation("activity_form.toast.create_success"))
                onSuccess(createdActivity)
                navigate(`/activities/${createdActivity.id}`)
                return
            }

            if (!initialData?.id) {
                toast.error(getTranslation("activity_form.toast.edit_impossible"))
                return
            }

            const updatedActivity = await updateActivity(initialData.id, payload)
            toast.success(getTranslation("activity_form.toast.edit_success"))
            onSuccess(updatedActivity)
            navigate(`/activities/${updatedActivity.id}`)
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

    return (
        <div className="max-w-4xl mx-auto w-full">
            <Card>
                <CardHeader>
                    <CardTitle>{getTranslation("activity_form.card_title")}</CardTitle>
                    <CardDescription>
                        {mode === "create"
                            ? getTranslation("activity_form.card_description_create")
                            : getTranslation("activity_form.card_description_edit")}
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-6">

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Title */}
                            <div className="space-y-1">
                                <Label htmlFor="title">{getTranslation("activity_form.title_label")}</Label>
                                <Input
                                    id="title"
                                    value={title}
                                    onChange={(e) => { setTitle(e.target.value); clearError("title") }}
                                    placeholder={getTranslation("activity_form.title_placeholder")}
                                    aria-invalid={!!errors.title}
                                    className={errors.title ? "border-red-500 focus-visible:ring-red-500" : ""}
                                />
                                <FieldError message={errors.title} />
                            </div>

                            {/* Price */}
                            <div className="space-y-1">
                                <Label htmlFor="estimatedPrice">{getTranslation("activity_form.price_label")}</Label>
                                <Input
                                    id="estimatedPrice"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={estimatedPrice}
                                    onChange={(e) => setEstimatedPrice(e.target.value)}
                                    placeholder={getTranslation("activity_form.price_placeholder")}
                                />
                            </div>
                        </div>

                        {/* Localisation */}
                        <div className="space-y-1">
                            <EnhancedLocationInput
                                value={localisationData}
                                onChange={(val) => { setLocalisationData(val); clearError("localisation") }}
                                required
                            />
                            <FieldError message={errors.localisation} />
                        </div>

                        {/* Description */}
                        <div className="space-y-1">
                            <Label htmlFor="description">{getTranslation("activity_form.description_label")}</Label>
                            <Textarea
                                id="description"
                                value={description}
                                onChange={(e) => { setDescription(e.target.value); clearError("description") }}
                                rows={4}
                                placeholder={getTranslation("activity_form.description_placeholder")}
                                aria-invalid={!!errors.description}
                                className={errors.description ? "border-red-500 focus-visible:ring-red-500" : ""}
                            />
                            <FieldError message={errors.description} />
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Date */}
                            <div className="space-y-1">
                                <Label>{getTranslation("activity_form.start_date_label")}</Label>
                                <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
                                    <PopoverTrigger asChild>
                                        <Button
                                            type="button"
                                            variant="outline"
                                            className={[
                                                "w-full justify-start text-left font-normal",
                                                errors.startAt ? "border-red-500 focus-visible:ring-red-500" : "",
                                            ].join(" ")}
                                        >
                                            <CalendarIcon className="mr-2 h-4 w-4" />
                                            {date
                                                ? format(date, "PPP", { locale: dateLocale })
                                                : <span className="text-muted-foreground">{getTranslation("activity_form.start_date_label")}</span>
                                            }
                                        </Button>
                                    </PopoverTrigger>
                                    <PopoverContent className="w-auto p-0" align="start">
                                        <Calendar
                                            mode="single"
                                            selected={date}
                                            onSelect={(newDate) => {
                                                setDate(newDate)
                                                clearError("startAt")
                                                setCalendarOpen(false)
                                            }}
                                            locale={dateLocale}
                                            disabled={(d) => d < new Date(new Date().setHours(0, 0, 0, 0))}
                                        />
                                    </PopoverContent>
                                </Popover>
                                <FieldError message={errors.startAt} />
                            </div>

                            {/* Hour */}
                            <div className="space-y-1">
                                <Label>{getTranslation("activity_form.start_time_label")}</Label>
                                <div className="relative">
                                    <Input
                                        type="time"
                                        step={300}
                                        name="startTime"
                                        value={time}
                                        onChange={(e) => { setTime(e.target.value); clearError("time") }}
                                        ref={timeInputRef}
                                        aria-label={getTranslation("activity_form.start_time_aria")}
                                        aria-invalid={!!errors.time}
                                        className={[
                                            "pr-10",
                                            errors.time ? "border-red-500 focus-visible:ring-red-500" : "",
                                        ].join(" ")}
                                    />
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="icon"
                                        className="absolute right-1 top-1/2 h-7 w-7 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                                        onClick={() => {
                                            if (!timeInputRef.current) return
                                                ; (timeInputRef.current as HTMLInputElement & { showPicker?: () => void }).showPicker?.()
                                            timeInputRef.current.focus()
                                        }}
                                        aria-label={getTranslation("activity_form.start_time_aria")}
                                    >
                                        <Clock className="h-4 w-4" />
                                    </Button>
                                </div>
                                <FieldError message={errors.time} />
                            </div>
                        </div>

                        <SubActivityManager subActivities={subActivities} onChange={setSubActivities} />
                        <EquipmentManager equipment={requiredEquipment} onChange={setRequiredEquipment} showChecklist={false} />

                        {/* Image */}
                        <div className="space-y-2">
                            <Label>{getTranslation("activity_form.image_label")}</Label>
                            {!image ? (
                                <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center">
                                    <Upload className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                                    <p className="text-sm text-gray-500 mb-2">{getTranslation("activity_form.image_upload_hint")}</p>
                                    <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" id="image-upload" />
                                    <Button type="button" variant="outline" onClick={() => document.getElementById("image-upload")?.click()}>
                                        {getTranslation("activity_form.image_choose_button")}
                                    </Button>
                                </div>
                            ) : (
                                <div className="relative">
                                    <img src={image} alt={getTranslation("activity_form.image_preview_alt")} className="w-full h-48 object-cover rounded-lg" />
                                    <Button type="button" variant="destructive" size="sm" onClick={removeImage} className="absolute top-2 right-2">
                                        <X className="w-4 h-4" />
                                    </Button>
                                </div>
                            )}
                        </div>

                        <div className="flex justify-end gap-4 pt-6">
                            <Button type="button" variant="outline" onClick={onBack}>
                                {getTranslation("activity_form.button_cancel")}
                            </Button>
                            <Button type="submit" disabled={isLoading}>
                                {isLoading
                                    ? mode === "create"
                                        ? getTranslation("activity_form.button_creating")
                                        : getTranslation("activity_form.button_saving")
                                    : mode === "create"
                                        ? getTranslation("activity_form.button_create")
                                        : getTranslation("activity_form.button_edit")}
                            </Button>
                        </div>
                    </form>
                </CardContent>
            </Card>
        </div>
    )
}
