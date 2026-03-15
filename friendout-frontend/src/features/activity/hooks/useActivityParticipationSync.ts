import { useCallback } from "react";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import { useActivityParticipation } from "@/features/participant/hooks/useActivityParticipation";
import type { UserActivityParticipation } from "@/features/participant/types/UserParticipation.type";
import {
    getSubActivitySelectedStatus as getSubActivitySelectedStatusUtil,
    getSubActivitiesSelectedStatus as getSubActivitiesSelectedStatusUtil,
} from "@/features/participant/utils/participationStatus.utils";

type SetActivityDetails = React.Dispatch<
    React.SetStateAction<ActivityDetails | undefined>
>;

/**
 * Wires participant participation actions to activity details state.
 * Keeps ActivityDetails in sync when user changes participation (main or sub-activities).
 */
export function useActivityParticipationSync(
    activityDetails: ActivityDetails | undefined,
    setActivityDetails: SetActivityDetails
) {
    const onMainSuccess = useCallback(
        (updated: UserActivityParticipation) => {
            setActivityDetails(prev =>
                prev
                    ? {
                          ...prev,
                          userMainParticipation: updated.userMainParticipation,
                          participants:
                              updated.mainActivityParticipants ?? prev.participants,
                      }
                    : prev
            );
        },
        [setActivityDetails]
    );

    const onSubSuccess = useCallback(
        (updated: UserActivityParticipation) => {
            setActivityDetails(prev => {
                if (!prev) return prev;
                const participantsBySubActivity = new Map<string, typeof prev.subActivities[number]["participants"]>();
                updated.subActivitiesParticipants?.forEach(participant => {
                    if (!participant.subActivityId) return;
                    const existing = participantsBySubActivity.get(participant.subActivityId) ?? [];
                    participantsBySubActivity.set(participant.subActivityId, [...existing, participant]);
                });

                return {
                    ...prev,
                    userSubActivitiesParticipations:
                        updated.userSubActivitiesParticipations ??
                        prev.userSubActivitiesParticipations,
                    subActivities: prev.subActivities.map(sa => {
                        const updatedParticipants = participantsBySubActivity.get(sa.id);
                        if (!updatedParticipants) return sa;
                        return {
                            ...sa,
                            participants: updatedParticipants,
                        };
                    }),
                };
            });
        },
        [setActivityDetails]
    );

    const participation = useActivityParticipation({
        activityId: activityDetails?.id ?? "",
        subActivityIds:
            activityDetails?.subActivities.map(sa => sa.id) ?? [],
        onMainParticipationSuccess: onMainSuccess,
        onSubActivitiesParticipationSuccess: onSubSuccess,
    });

    const getSubActivitySelectedStatus = useCallback(
        (subActivityId: string) =>
            getSubActivitySelectedStatusUtil(
                activityDetails?.userSubActivitiesParticipations,
                subActivityId
            ),
        [activityDetails?.userSubActivitiesParticipations]
    );

    const getSubActivitiesSelectedStatus = useCallback(
        () =>
            getSubActivitiesSelectedStatusUtil(
                activityDetails?.userSubActivitiesParticipations,
                activityDetails?.subActivities.length ?? 0
            ),
        [
            activityDetails?.userSubActivitiesParticipations,
            activityDetails?.subActivities.length,
        ]
    );

    return {
        handleMainParticipationChange: participation.handleMainParticipationChange,
        handleSubActivitiesParticipationChange:
            participation.handleSubActivitiesParticipationChange,
        getSubActivitySelectedStatus,
        getSubActivitiesSelectedStatus,
    };
}
