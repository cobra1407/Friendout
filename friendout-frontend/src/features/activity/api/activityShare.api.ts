import api from "@/lib/api/api";
import type { PublicActivity } from "@/features/activity/types/publicActivity.type";

export interface ShareLink {
    shareToken: string;
}

/**
 * Returns the activity's public share link, generating one on first use.
 * Any participant (or the creator) can call this — idempotent, so it's safe
 * to call every time the user clicks "Share".
 */
export async function getOrCreateShareLink(activityId: string): Promise<ShareLink> {
    const response = await api.post<ShareLink>(`/activities/${activityId}/share`);
    return response.data;
}

/**
 * Fetches the read-only public view of an activity by its share token.
 * Unauthenticated endpoint — used by the public /share/:shareToken page.
 */
export async function getPublicActivity(shareToken: string): Promise<PublicActivity> {
    const response = await api.get<PublicActivity>(`/public/activities/${shareToken}`);
    return response.data;
}
