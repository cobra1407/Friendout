import type { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";

type ParticipationWithStatus = { subActivityId?: string; status: ParticipationStatus };

/**
 * Returns the participation status for a given sub-activity.
 */
export function getSubActivitySelectedStatus(
    participations: ParticipationWithStatus[] | undefined,
    subActivityId: string
): ParticipationStatus | null {
    const participation = participations?.find(
        p => p.subActivityId === subActivityId
    );
    return participation?.status ?? null;
}

/**
 * Returns the participation status if all sub-activities have a participation
 * and share the same status; otherwise returns null.
 */
export function getSubActivitiesSelectedStatus(
    participations: ParticipationWithStatus[] | undefined,
    subActivitiesCount: number
): ParticipationStatus | null {
    if (!participations?.length || participations.length !== subActivitiesCount) {
        return null;
    }
    const firstStatus = participations[0].status;
    const sameStatus = participations.every(p => p.status === firstStatus);
    return sameStatus ? firstStatus : null;
}
