export interface UserProfile {
    name: string
    email: string | null
    avatarUrl: string | null
    hasCustomAvatar: boolean
}

export interface UpdateUserProfilePayload {
    name: string
}
