import { useCallback } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getActivityById } from "@/features/activity/api/activity.api";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";

export const activityDetailsQueryKey = (id: string) => ["activity", id] as const;

export function useActivityDetails(id: string | undefined) {
    const queryClient = useQueryClient();

    const { data: activityDetails, isLoading } = useQuery({
        queryKey: activityDetailsQueryKey(id ?? ""),
        queryFn: () => getActivityById(id!),
        enabled: !!id,
    });
    
    const setActivityDetails = useCallback(
        (
            updater:
                | ActivityDetails
                | undefined
                | ((prev: ActivityDetails | undefined) => ActivityDetails | undefined)
        ) => {
            if (!id) return;
            queryClient.setQueryData<ActivityDetails | undefined>(activityDetailsQueryKey(id), updater);
        },
        [queryClient, id]
    );

    return { activityDetails, setActivityDetails, isLoading };
}
