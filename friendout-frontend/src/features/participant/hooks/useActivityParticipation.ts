import { useMutation } from "@tanstack/react-query";
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
    const mainMutation = useMutation({
        mutationFn: (status: ParticipationStatus) =>
            UpsertParticipation({ activityId, status, subActivityIds: null }),
        onSuccess: onMainParticipationSuccess,
        onError: () => toast.error("Impossible de mettre à jour la participation"),
    });

    const subMutation = useMutation({
        mutationFn: ({ status, ids }: { status: ParticipationStatus; ids?: string[] }) =>
            UpsertParticipation({ activityId, status, subActivityIds: ids ?? subActivityIds }),
        onSuccess: onSubActivitiesParticipationSuccess,
        onError: () => toast.error("Impossible de mettre à jour la participation"),
    });

    return {
        handleMainParticipationChange: (status: ParticipationStatus) => mainMutation.mutate(status),
        handleSubActivitiesParticipationChange: (status: ParticipationStatus, ids?: string[]) =>
            subMutation.mutate({ status, ids }),
    };
}
