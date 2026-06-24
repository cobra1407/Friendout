import { useRef, useState } from "react"
import { Camera, RotateCcw } from "lucide-react"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Spinner } from "@/components/ui/spinner"
import { FieldError } from "@/components/ui/FieldError"
import { getTranslation } from "@/i18n"
import { LanguageField } from "@/features/preferences/components/LanguageField"
import { profileNameSchema } from "@/features/preferences/schema/profile.schema"
import type { UserProfile } from "@/features/preferences/types/profile.type"
import type { SupportedLocale } from "@/features/preferences/types/preferences.type"

interface PreferencesProfileSectionProps {
    profile: UserProfile | undefined
    isLoading: boolean
    isSavingName: boolean
    isUploadingAvatar: boolean
    isResettingAvatar: boolean
    onSaveName: (name: string) => void
    onUploadAvatar: (file: File) => void
    onResetAvatar: () => void
    locale: SupportedLocale | undefined
    isLocaleLoading: boolean
    isLocaleSaving: boolean
    onChangeLocale: (locale: SupportedLocale) => void
}

export const PreferencesProfileSection = ({
    profile,
    isLoading,
    isSavingName,
    isUploadingAvatar,
    isResettingAvatar,
    onSaveName,
    onUploadAvatar,
    onResetAvatar,
    locale,
    isLocaleLoading,
    isLocaleSaving,
    onChangeLocale,
}: PreferencesProfileSectionProps) => {
    const fileInputRef = useRef<HTMLInputElement>(null)
    const [name, setName] = useState(profile?.name ?? "")
    const [nameTouched, setNameTouched] = useState(false)
    const [nameError, setNameError] = useState<string | undefined>(undefined)

    // what the user is actively typing.
    if (profile && !nameTouched && name !== profile.name) {
        setName(profile.name)
    }

    const initials = (profile?.name ?? "?")
        .split(" ")
        .map((part) => part[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)

    const handleNameBlur = () => {
        if (!profile) return

        const result = profileNameSchema.safeParse(name)
        if (!result.success) {
            setNameError(getTranslation(`preferences.profile.${result.error.issues[0].message}`))
            return
        }

        setNameError(undefined)
        const trimmed = result.data
        if (!trimmed || trimmed === profile.name) return
        onSaveName(trimmed)
    }

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0]
        if (file) onUploadAvatar(file)
        event.target.value = ""
    }

    const avatarBusy = isUploadingAvatar || isResettingAvatar

    if (isLoading) {
        return (
            <div className="rounded-xl border bg-card overflow-hidden">
                <div className="h-28 sm:h-36 bg-muted animate-pulse" />
                <div className="flex justify-center py-8"><Spinner /></div>
            </div>
        )
    }

    return (
        <div className="rounded-xl border bg-card overflow-hidden">
            {/* Banner */}
            <div className="h-28 sm:h-36 bg-gradient-to-r from-primary/25 via-primary/10 to-transparent relative" />

            <div className="px-4 sm:px-6 pb-6">
                {/* Avatar overlapping the banner */}
                <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3 -mt-10 sm:-mt-12 mb-4">
                    <div className="relative w-fit">
                        <Avatar className="size-20 sm:size-24 ring-4 ring-card shadow-sm">
                            <AvatarImage src={profile?.avatarUrl ?? undefined} alt={profile?.name} />
                            <AvatarFallback className="text-xl font-bold bg-avatar text-white">
                                {initials}
                            </AvatarFallback>
                        </Avatar>
                        <button
                            type="button"
                            onClick={() => fileInputRef.current?.click()}
                            disabled={avatarBusy}
                            className="absolute -bottom-1 -right-1 flex items-center justify-center size-7 rounded-full bg-primary text-primary-foreground shadow-sm ring-2 ring-card hover:opacity-90 transition-opacity disabled:opacity-50 cursor-pointer"
                            title={getTranslation("preferences.profile.avatar_upload_button")}
                        >
                            {isUploadingAvatar ? <Spinner className="size-3.5" /> : <Camera className="size-3.5" />}
                        </button>
                        <input
                            ref={fileInputRef}
                            type="file"
                            accept="image/*"
                            onChange={handleFileChange}
                            className="hidden"
                        />
                    </div>

                    {profile?.hasCustomAvatar && (
                        <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            className="text-xs w-fit"
                            onClick={onResetAvatar}
                            disabled={avatarBusy}
                        >
                            <RotateCcw className="size-3" />
                            {getTranslation("preferences.profile.avatar_reset_button")}
                        </Button>
                    )}
                </div>

                {/* Name + email summary */}
                <div className="mb-6">
                    <h2 className="text-lg font-bold text-foreground">{profile?.name}</h2>
                    {profile?.email && (
                        <p className="text-sm text-muted-foreground">{profile.email}</p>
                    )}
                    <p className="text-xs text-muted-foreground mt-1">
                        {getTranslation("preferences.profile.avatar_hint")}
                    </p>
                </div>

                {/* Edit form */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                        <Label htmlFor="profile-name" className="text-sm font-medium">
                            {getTranslation("preferences.profile.name_label")}
                        </Label>
                        <Input
                            id="profile-name"
                            value={name}
                            onChange={(e) => { setName(e.target.value); setNameTouched(true); setNameError(undefined) }}
                            onBlur={handleNameBlur}
                            disabled={isSavingName}
                            maxLength={191}
                            aria-invalid={!!nameError}
                        />
                        <FieldError message={nameError} />
                    </div>

                    <div className="space-y-1.5">
                        <Label className="text-sm font-medium">
                            {getTranslation("preferences.profile.email_label")}
                        </Label>
                        <Input value={profile?.email ?? ""} disabled readOnly />
                        <p className="text-xs text-muted-foreground">
                            {getTranslation("preferences.profile.email_hint")}
                        </p>
                    </div>

                    <LanguageField
                        locale={locale}
                        isLoading={isLocaleLoading}
                        disabled={isLocaleSaving}
                        onChange={onChangeLocale}
                    />
                </div>
            </div>
        </div>
    )
}
