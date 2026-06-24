import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { getTranslation } from "@/i18n"
import { useAuth } from "@/features/auth/hooks/useAuth"
import {
    getUserProfile,
    resetUserAvatar,
    updateUserProfile,
    uploadUserAvatar,
} from "@/features/preferences/api/profile.api"
import type { UpdateUserProfilePayload, UserProfile } from "@/features/preferences/types/profile.type"

const PROFILE_KEY = ["profile", "me"]

export function useProfile() {
    const qc = useQueryClient()
    const { updateUser } = useAuth()

    const { data: profile, isLoading } = useQuery({
        queryKey: PROFILE_KEY,
        queryFn: getUserProfile,
    })

    const applyResult = (data: UserProfile) => {
        qc.setQueryData(PROFILE_KEY, data)
        // Keep the header/menu avatar + name in sync without a full refetch.
        updateUser({ name: data.name, avatarUrl: data.avatarUrl ?? undefined })
    }

    const { mutate: saveName, isPending: isSavingName } = useMutation({
        mutationFn: (payload: UpdateUserProfilePayload) => updateUserProfile(payload),
        onSuccess: applyResult,
        onError: () => toast.error(getTranslation("preferences.toast_error")),
    })

    const { mutate: uploadAvatar, isPending: isUploadingAvatar } = useMutation({
        mutationFn: (file: File) => uploadUserAvatar(file),
        onSuccess: applyResult,
        onError: () => toast.error(getTranslation("preferences.profile.avatar_upload_error")),
    })

    const { mutate: resetAvatar, isPending: isResettingAvatar } = useMutation({
        mutationFn: () => resetUserAvatar(),
        onSuccess: applyResult,
        onError: () => toast.error(getTranslation("preferences.toast_error")),
    })

    return {
        profile,
        isLoading,
        saveName: (name: string) => saveName({ name }),
        isSavingName,
        uploadAvatar,
        isUploadingAvatar,
        resetAvatar,
        isResettingAvatar,
    }
}
