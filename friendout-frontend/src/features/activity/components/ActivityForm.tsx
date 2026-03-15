import { useRef, useState } from "react"
import { useNavigate } from "react-router-dom"
import { Upload, X, CalendarIcon } from "lucide-react"
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
import EquipmentManager from "@/features/equipment/component/EquipmentManager"
import SubActivityManager from "@/features/subActivity/component/SubActivityManager"
import type { Activity } from "@/features/activity/types/acitivity.type"
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type"
import type { SubActivity } from "@/features/subActivity/types/subActivity.type"
import { LocalisationType } from "@/features/localisation/types/localisation.type"
import type { Localisation } from "@/features/localisation/types/localisation.type"
import { resolveMediaUrl } from "@/lib/media"
import { pickLocalisation } from "@/features/activity/utils/localisation.utils"
import { getTranslation, getLang } from "@/i18n"

const t = getTranslation
const dateLocale = getLang() === "fr" ? fr : enUS

interface ActivityFormProps {
    mode: "create" | "edit"
    initialData?: Activity | ActivityDetails
    onBack: () => void
    onSuccess: (activity: Activity) => void
}

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

const toLocalDateTimeString = (value: Date): string => {
    const pad = (n: number) => n.toString().padStart(2, "0")
    return [
        value.getFullYear(), "-",
        pad(value.getMonth() + 1), "-",
        pad(value.getDate()), "T",
        pad(value.getHours()), ":",
        pad(value.getMinutes()), ":",
        pad(value.getSeconds()),
    ].join("")
}

const normalizeSubActivitiesForForm = (subActivities: SubActivity[] | undefined): SubActivity[] => {
    if (!subActivities) return []
    return subActivities.map((subActivity) => ({
        ...subActivity,
        localisation: pickLocalisation(subActivity as SubActivity & { location?: Localisation | null }),
        startTime: formatToHHmm(subActivity.startTime),
        endTime: formatToHHmm(subActivity.endTime),
    }))
}

const getInitialLocalisation = (initialData: Activity | ActivityDetails | undefined): Localisation | null => {
    return pickLocalisation(initialData as (Activity | ActivityDetails) & { location?: Localisation | null })
}

const getInitialRequiredEquipment = (initialData: Activity | ActivityDetails | undefined): string[] => {
    if (!initialData) return []
    if ("requiredEquipments" in initialData && Array.isArray(initialData.requiredEquipments)) {
        return initialData.requiredEquipments.map((item) => item.name).filter(Boolean)
    }
    return []
}

export default function ActivityForm({ mode, initialData, onBack, onSuccess }: ActivityFormProps) {
    const navigate = useNavigate()
    const timeInputRef = useRef<HTMLInputElement | null>(null)

    const [isLoading, setIsLoading] = useState(false)
    const [title, setTitle] = useState(initialData?.title ?? "")
    const [description, setDescription] = useState(initialData?.description ?? "")
    const [date, setDate] = useState<Date | undefined>(initialData?.startAt ? new Date(initialData.startAt) : undefined)
    const [calendarOpen, setCalendarOpen] = useState(false)
    const [time, setTime] = useState(initialData?.startAt ? formatToHHmm(initialData.startAt) : "")
    const [estimatedPrice, setEstimatedPrice] = useState(
        initialData?.estimatedPrice !== undefined && initialData.estimatedPrice !== null
            ? String(initialData.estimatedPrice)
            : "",
    )
    const [localisationData, setLocalisationData] = useState<Localisation | null>(getInitialLocalisation(initialData))
    const [requiredEquipment, setRequiredEquipment] = useState<string[]>(getInitialRequiredEquipment(initialData))
    const [imageFile, setImageFile] = useState<File | null>(null)
    const [image, setImage] = useState<string | null>(formatImageUrl(initialData?.image?.url ?? null))
    const [subActivities, setSubActivities] = useState<SubActivity[]>(normalizeSubActivitiesForForm(initialData?.subActivities))

    const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]
        if (!file) return
        const limitSize = 10 * 1024 * 1024
        if (file.size > limitSize) return toast.error(t("activity_form.toast.image_too_large", { size: String(limitSize / 1024 / 1024) }))
        if (!file.type.startsWith("image/")) return toast.error(t("activity_form.toast.image_invalid_type"))
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

    const toMinutes = (timeValue: string): number => {
        const [hours, minutes] = timeValue.split(":").map(Number)
        if (!Number.isFinite(hours) || !Number.isFinite(minutes)) return Number.NaN
        return hours * 60 + minutes
    }

    const validateForm = (): boolean => {
        if (!title.trim()) return toast.error(t("activity_form.toast.title_required")), false
        if (!description.trim()) return toast.error(t("activity_form.toast.description_required")), false
        if (!date) return toast.error(t("activity_form.toast.date_required")), false
        if (!time) return toast.error(t("activity_form.toast.time_required")), false
        if (!/^\d{2}:\d{2}$/.test(time)) return toast.error(t("activity_form.toast.time_invalid_format")), false
        if (!localisationData) return toast.error(t("activity_form.toast.location_required")), false

        const hasLocalisationValue =
            Boolean(localisationData.address?.trim()) ||
            Boolean(localisationData.mapLink?.trim()) ||
            localisationData.type === LocalisationType.Virtual
        if (!hasLocalisationValue) return toast.error(t("activity_form.toast.location_incomplete")), false

        for (let index = 0; index < subActivities.length; index += 1) {
            const subActivity = subActivities[index]
            const position = String(index + 1)

            if (!subActivity.name?.trim()) return toast.error(t("activity_form.toast.sub_activity_name_required", { position })), false
            if (!subActivity.startTime) return toast.error(t("activity_form.toast.sub_activity_start_required", { position })), false
            if (!subActivity.endTime) return toast.error(t("activity_form.toast.sub_activity_end_required", { position })), false

            const startMinutes = toMinutes(subActivity.startTime)
            const endMinutes = toMinutes(subActivity.endTime)
            if (!Number.isFinite(startMinutes) || !Number.isFinite(endMinutes)) {
                return toast.error(t("activity_form.toast.sub_activity_time_invalid", { position })), false
            }
            if (endMinutes <= startMinutes) {
                return toast.error(t("activity_form.toast.sub_activity_end_before_start", { position })), false
            }
            if (subActivity.localisation) {
                const hasSubLocalisationValue =
                    Boolean(subActivity.localisation.address?.trim()) ||
                    Boolean(subActivity.localisation.mapLink?.trim()) ||
                    Boolean(subActivity.localisation.virtualUrl?.trim()) ||
                    Boolean(subActivity.localisation.serverInfo?.trim())
                if (!hasSubLocalisationValue) {
                    return toast.error(t("activity_form.toast.sub_activity_location_incomplete", { position })), false
                }
            }
        }

        return true
    }

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault()
        setIsLoading(true)
        if (!validateForm()) {
            setIsLoading(false)
            return
        }

        const startAt = new Date(date!)
        startAt.setHours(Number(time.split(":")[0]), Number(time.split(":")[1]), 0, 0)
        if (mode === "create" && startAt <= new Date()) {
            toast.error(t("activity_form.toast.date_must_be_future"))
            setIsLoading(false)
            return
        }

        try {
            const payload = {
                title,
                description,
                startAt: toLocalDateTimeString(startAt),
                time,
                estimatedPrice: estimatedPrice ? parseFloat(estimatedPrice) : undefined,
                localisation: localisationData,
                activityImage: imageFile ?? undefined,
                requiredEquipmentNames: requiredEquipment,
                subActivities,
            }

            if (mode === "create") {
                const createdActivity = await createActivity(payload)
                toast.success(t("activity_form.toast.create_success"))
                onSuccess(createdActivity)
                navigate(`/activities/${createdActivity.id}`)
                return
            }

            if (!initialData?.id) {
                toast.error(t("activity_form.toast.edit_impossible"))
                return
            }

            const updatedActivity = await updateActivity(initialData.id, payload)
            toast.success(t("activity_form.toast.edit_success"))
            onSuccess(updatedActivity)
            navigate(`/activities/${updatedActivity.id}`)
        } catch (error: unknown) {
            console.error(error)
            if (axios.isAxiosError(error)) {
                const message = typeof error.response?.data === "string" ? error.response.data : (error.response?.data?.errorMessage as string | undefined)
                toast.error(message || t("activity_form.toast.save_error"))
            } else {
                toast.error(t("activity_form.toast.save_error"))
            }
        } finally {
            setIsLoading(false)
        }
    }

    return (
        <div className="max-w-4xl mx-auto w-full">
            <Card>
                <CardHeader>
                    <CardTitle>{t("activity_form.card_title")}</CardTitle>
                    <CardDescription>
                        {mode === "create"
                            ? t("activity_form.card_description_create")
                            : t("activity_form.card_description_edit")}
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="space-y-2">
                                <Label htmlFor="title">{t("activity_form.title_label")}</Label>
                                <Input
                                    id="title"
                                    value={title}
                                    onChange={(e) => setTitle(e.target.value)}
                                    placeholder={t("activity_form.title_placeholder")}
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="estimatedPrice">{t("activity_form.price_label")}</Label>
                                <Input
                                    id="estimatedPrice"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={estimatedPrice}
                                    onChange={(e) => setEstimatedPrice(e.target.value)}
                                    placeholder={t("activity_form.price_placeholder")}
                                />
                            </div>
                        </div>

                        <EnhancedLocationInput value={localisationData} onChange={setLocalisationData} required />

                        <div className="space-y-2">
                            <Label htmlFor="description">{t("activity_form.description_label")}</Label>
                            <Textarea
                                id="description"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                rows={4}
                                placeholder={t("activity_form.description_placeholder")}
                            />
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Calendar */}
                            <div className="space-y-2">
                                <Label>{t("activity_form.start_date_label")}</Label>
                                <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
                                    <PopoverTrigger asChild>
                                        <Button
                                            type="button"
                                            variant="outline"
                                            className="w-full justify-start text-left font-normal"
                                        >
                                            <CalendarIcon className="mr-2 h-4 w-4" />
                                            {date
                                                ? format(date, "PPP", { locale: dateLocale })
                                                : <span className="text-muted-foreground">{t("activity_form.start_date_label")}</span>
                                            }
                                        </Button>
                                    </PopoverTrigger>
                                    <PopoverContent className="w-auto p-0" align="start">
                                        <Calendar
                                            mode="single"
                                            selected={date}
                                            onSelect={(newDate) => {
                                                setDate(newDate)
                                                setCalendarOpen(false)
                                            }}
                                            locale={dateLocale}
                                            disabled={(d) => d < new Date(new Date().setHours(0, 0, 0, 0))}
                                        />
                                    </PopoverContent>
                                </Popover>
                            </div>

                            {/* Time picker */}
                            <div className="space-y-2">
                                <Label>{t("activity_form.start_time_label")}</Label>
                                <div className="relative">
                                    <Input
                                        type="time"
                                        value={time}
                                        onChange={(e) => setTime(e.target.value)}
                                        className="pr-8 [&::-webkit-calendar-picker-indicator]:invert"
                                        ref={timeInputRef}
                                    />
                                    <button
                                        type="button"
                                        className="absolute top-1/2 right-2 -translate-y-1/2 w-6 h-6 flex items-center justify-center text-gray-400 hover:text-gray-600"
                                        onClick={() => {
                                            if (!timeInputRef.current) return
                                                ; (timeInputRef.current as any).showPicker?.()
                                            timeInputRef.current.focus()
                                        }}
                                        aria-label={t("activity_form.start_time_aria")}
                                    >
                                    </button>
                                </div>
                            </div>
                        </div>

                        <SubActivityManager subActivities={subActivities} onChange={setSubActivities} />
                        <EquipmentManager equipment={requiredEquipment} onChange={setRequiredEquipment} showChecklist={false} />

                        <div className="space-y-2">
                            <Label>{t("activity_form.image_label")}</Label>
                            {!image ? (
                                <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center">
                                    <Upload className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                                    <p className="text-sm text-gray-500 mb-2">{t("activity_form.image_upload_hint")}</p>
                                    <input type="file" accept="image/*" onChange={handleImageUpload} className="hidden" id="image-upload" />
                                    <Button type="button" variant="outline" onClick={() => document.getElementById("image-upload")?.click()}>
                                        {t("activity_form.image_choose_button")}
                                    </Button>
                                </div>
                            ) : (
                                <div className="relative">
                                    <img src={image} alt={t("activity_form.image_preview_alt")} className="w-full h-48 object-cover rounded-lg" />
                                    <Button type="button" variant="destructive" size="sm" onClick={removeImage} className="absolute top-2 right-2">
                                        <X className="w-4 h-4" />
                                    </Button>
                                </div>
                            )}
                        </div>

                        <div className="flex justify-end gap-4 pt-6">
                            <Button type="button" variant="outline" onClick={onBack}>
                                {t("activity_form.button_cancel")}
                            </Button>
                            <Button type="submit" disabled={isLoading}>
                                {isLoading
                                    ? mode === "create"
                                        ? t("activity_form.button_creating")
                                        : t("activity_form.button_saving")
                                    : mode === "create"
                                        ? t("activity_form.button_create")
                                        : t("activity_form.button_edit")}
                            </Button>
                        </div>
                    </form>
                </CardContent>
            </Card>
        </div>
    )
}
