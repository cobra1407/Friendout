import { useMutation, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { toast } from "sonner";
import { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";
import { UpsertParticipation } from "@/features/participant/api/participant.api";
import type { UserActivityParticipation } from "@/features/participant/types/UserParticipation.type";
import { getTranslation } from "@/i18n";

/**
 * Message returned by the API (ParticipantService.SaveParticipationAsync) when the participant
 * limit is reached. The controller returns the raw message inside a BadRequest.
 */
const ACTIVITY_FULL_MESSAGE = "This activity is full";

const handleParticipationError = (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.data === ACTIVITY_FULL_MESSAGE) {
        toast.error(getTranslation("participation.activity_full"));
        return;
    }
    toast.error("Impossible de mettre à jour la participation");
};

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
    const queryClient = useQueryClient();

    // The spot counter in the list comes from the "activities" query: we mark it
    // as stale so that it is refetched upon returning to the list.
    const invalidateActivitiesList = () =>
        queryClient.invalidateQueries({ queryKey: ["activities"] });

    const mainMutation = useMutation({
        mutationFn: (status: ParticipationStatus) =>
            UpsertParticipation({ activityId, status, subActivityIds: null }),
        onSuccess: (result) => {
            onMainParticipationSuccess(result);
            invalidateActivitiesList();
        },
        onError: handleParticipationError,
    });

    const subMutation = useMutation({
        mutationFn: ({ status, ids }: { status: ParticipationStatus; ids?: string[] }) =>
            UpsertParticipation({ activityId, status, subActivityIds: ids ?? subActivityIds }),
        onSuccess: onSubActivitiesParticipationSuccess,
        onError: handleParticipationError,
    });

    return {
        handleMainParticipationChange: (status: ParticipationStatus) => mainMutation.mutate(status),
        handleSubActivitiesParticipationChange: (status: ParticipationStatus, ids?: string[]) =>
            subMutation.mutate({ status, ids }),
    };
}
