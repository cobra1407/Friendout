import { useEffect, useState } from "react";
import { getActivityById } from "@/features/activity/api/activity.api";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";

export function useActivityDetails(id: string | undefined) {
    const [activityDetails, setActivityDetails] = useState<ActivityDetails>();
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        if (!id) return;

        const fetchData = async () => {
            try {
                setIsLoading(true);
                const activity = await getActivityById(id);
                setActivityDetails(activity);
            } catch (err) {
                if (import.meta.env.DEV) {
                    console.error("Erreur fetch activité:", err);
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchData();
    }, [id]);

    return { activityDetails, setActivityDetails, isLoading };
}
