import api from "@/lib/api/api"
import type { UpdateUserPreferencesPayload, UserPreferences } from "@/features/preferences/types/preferences.type"

export async function getMyPreferences(): Promise<UserPreferences> {
    const response = await api.get<UserPreferences>("/preferences/me")
    return response.data
}

export async function updateMyPreferences(payload: UpdateUserPreferencesPayload): Promise<UserPreferences> {
    const response = await api.put<UserPreferences>("/preferences/me", payload)
    return response.data
}
