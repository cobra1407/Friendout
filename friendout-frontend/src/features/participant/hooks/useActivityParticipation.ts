import { useCallback } from "react";
import { toast } from "sonner";
import { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";
import { UpsertParticipation } from "@/features/participant/api/participant.api";
import type { UserActivityParticipation } from "@/features/participant/types/UserParticipation.type";

export interface UseActivityParticipationParams {
    activityId: string;
    subActivityIds: string[];
    onMainParticipationSuccess: (result: UserActivityParticipation) => void;
    onSubActivitiesParticipationSuccess: (
        result: UserActivityParticipation
    ) => void;
}

export function useActivityParticipation({
    activityId,
    subActivityIds,
    onMainParticipationSuccess,
    onSubActivitiesParticipationSuccess,
}: UseActivityParticipationParams) {
    const handleMainParticipationChange = useCallback(
        async (status: ParticipationStatus) => {
            try {
                const updated = await UpsertParticipation({
                    activityId,
                    status,
                    subActivityIds: null,
                });
                onMainParticipationSuccess(updated);
            } catch {
                toast.error("Impossible de mettre à jour la participation");
            }
        },
        [
            activityId,
            onMainParticipationSuccess,
        ]
    );

    const handleSubActivitiesParticipationChange = useCallback(
        async (status: ParticipationStatus, ids?: string[]) => {
            try {
                const updated = await UpsertParticipation({
                    activityId,
                    status,
                    subActivityIds: ids ?? subActivityIds,
                });
                onSubActivitiesParticipationSuccess(updated);
            } catch {
                toast.error("Impossible de mettre à jour la participation");
            }
        },
        [activityId, subActivityIds, onSubActivitiesParticipationSuccess]
    );

    return {
        handleMainParticipationChange,
        handleSubActivitiesParticipationChange,
    };
}
