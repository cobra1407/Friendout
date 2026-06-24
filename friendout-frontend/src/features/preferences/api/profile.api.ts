import api from "@/lib/api/api"
import type { UpdateUserProfilePayload, UserProfile } from "@/features/preferences/types/profile.type"

export async function getUserProfile(): Promise<UserProfile> {
    const response = await api.get<UserProfile>("/profile/me")
    return response.data
}

export async function updateUserProfile(payload: UpdateUserProfilePayload): Promise<UserProfile> {
    const response = await api.put<UserProfile>("/profile/me", payload)
    return response.data
}

export async function uploadUserAvatar(file: File): Promise<UserProfile> {
    const formData = new FormData()
    formData.append("Avatar", file)
    const response = await api.post<UserProfile>("/profile/me/avatar", formData)
    return response.data
}

export async function resetUserAvatar(): Promise<UserProfile> {
    const response = await api.post<UserProfile>("/profile/me/avatar/reset")
    return response.data
}
