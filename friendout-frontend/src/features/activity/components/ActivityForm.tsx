import { CalendarIcon, Clock, Upload, X } from "lucide-react"
import { format } from "date-fns"
import { fr, enUS } from "date-fns/locale"

import EnhancedLocationInput from "@/components/EnhancedLocationInput"
import { FieldError } from "@/components/ui/FieldError"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Calendar } from "@/components/ui/calendar"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"

import EquipmentManager from "@/features/equipment/component/EquipmentManager"
import SubActivityManager from "@/features/subActivity/component/SubActivityManager"
import type { Activity } from "@/features/activity/types/activity.type"
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type"
import { useActivityForm } from "@/features/activity/hooks/useActivityForm"
import { getTranslation, getLang } from "@/i18n"
import { EditConfirmationModal } from "./EditConfirmationModal"

const dateLocale = getLang() === "fr" ? fr : enUS

interface ActivityFormProps {
    mode: "create" | "edit"
    initialData?: Activity | ActivityDetails
    onBack: () => void
    onSuccess: (activity: Activity) => void
}

export default function ActivityForm({ mode, initialData, onBack, onSuccess }: ActivityFormProps) {
    const {
        isLoading, errors,
        title, setTitle,
        description, setDescription,
        date, setDate,
        calendarOpen, setCalendarOpen,
        time, setTime,
        estimatedPrice, setEstimatedPrice,
        localisationData, handleLocalisationChange,
        requiredEquipment, setRequiredEquipment,
        image,
        subActivities, setSubActivities,
        timeInputRef,
        clearError,
        handleImageUpload,
        removeImage,
        handleSubmit,
        handleConfirmUpdate,
        handleCancelUpdate,
        showConfirmModal,
        fieldRefs,
    } = useActivityForm({ mode, initialData, onSuccess })

    return (
        <>
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
                                <div className="space-y-1" ref={fieldRefs.title}>
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

                            {/* Location */}
                            <div className="space-y-1" ref={fieldRefs.localisation}>
                                <EnhancedLocationInput
                                    value={localisationData}
                                    onChange={handleLocalisationChange}
                                    required
                                />
                                <FieldError message={errors.localisation} />
                            </div>

                            {/* Description */}
                            <div className="space-y-1" ref={fieldRefs.description}>
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
                                <div className="space-y-1" ref={fieldRefs.startAt}>
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

                                {/* Time */}
                                <div className="space-y-1" ref={fieldRefs.time}>
                                    <Label>{getTranslation("activity_form.start_time_label")}</Label>
                                    <div className="relative">
                                        <Input
                                            type="time"
                                            step={300}
                                            name="startTime"
                                            value={time}
                                            onChange={(e) => { setTime(e.target.value); clearError("time") }}
                                            onMouseDown={(e) => {
                                                const input = e.currentTarget
                                                const rect = input.getBoundingClientRect()
                                                const clickX = e.clientX - rect.left
                                                if (clickX > 70) {
                                                    e.preventDefault()
                                                    const el = input as HTMLInputElement & { showPicker?: () => void }
                                                    el.focus()
                                                    el.showPicker?.()
                                                }
                                            }}
                                            ref={timeInputRef}
                                            aria-label={getTranslation("activity_form.start_time_aria")}
                                            aria-invalid={!!errors.time}
                                            className={[
                                                "pr-10 [&::-webkit-calendar-picker-indicator]:hidden [&::-webkit-calendar-picker-indicator]:appearance-none",
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
                                        <p className="text-sm text-gray-500 mb-2">
                                            {getTranslation("activity_form.image_upload_hint")}
                                        </p>
                                        <input
                                            type="file"
                                            accept="image/*"
                                            onChange={handleImageUpload}
                                            className="hidden"
                                            id="image-upload"
                                        />
                                        <Button
                                            type="button"
                                            variant="outline"
                                            onClick={() => document.getElementById("image-upload")?.click()}
                                        >
                                            {getTranslation("activity_form.image_choose_button")}
                                        </Button>
                                    </div>
                                ) : (
                                    <div className="relative">
                                        <img
                                            src={image}
                                            alt={getTranslation("activity_form.image_preview_alt")}
                                            className="w-full h-48 object-cover rounded-lg"
                                        />
                                        <Button
                                            type="button"
                                            variant="destructive"
                                            size="sm"
                                            onClick={removeImage}
                                            className="absolute top-2 right-2"
                                        >
                                            <X className="w-4 h-4" />
                                        </Button>
                                    </div>
                                )}
                            </div>

                            {/* Actions */}
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

            {/* Confirmation Modal */}
            <EditConfirmationModal
                showConfirmModal={showConfirmModal}
                isLoading={isLoading}
                handleCancelUpdate={handleCancelUpdate}
                handleConfirmUpdate={handleConfirmUpdate}
            />
        </>
    )
}
