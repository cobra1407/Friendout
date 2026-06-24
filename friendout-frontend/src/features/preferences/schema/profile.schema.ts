import { z } from "zod"

// Mirrors the backend validation in UserService.UpdateMyProfileAsync (trimmed, non-empty,
// max 191 chars to match the `name` column length) so the user gets the same feedback
// instantly client-side instead of waiting for a round trip.
export const profileNameSchema = z
    .string()
    .trim()
    .min(1, "name_required")
    .max(191, "name_too_long")

export type ProfileNameFormData = z.infer<typeof profileNameSchema>
