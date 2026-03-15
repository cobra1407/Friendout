import { useCallback } from "react";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import { useActivityEquipment } from "@/features/equipment/hooks/useActivityEquipment";

type SetActivityDetails = React.Dispatch<
    React.SetStateAction<ActivityDetails | undefined>
>;

/**
 * Connecte les actions équipement (feature equipment) à l'état ActivityDetails.
 */
export function useActivityEquipmentSync(
    activityDetails: ActivityDetails | undefined,
    setActivityDetails: SetActivityDetails
) {
    const onQuantityUpdated = useCallback(
        (userEquipments: ActivityDetails["userEquipments"]) => {
            setActivityDetails(prev =>
                prev
                    ? {
                        ...prev,
                        userEquipments: userEquipments.filter((userEquipment) =>
                            prev.requiredEquipments.some(
                                (requiredEquipment) => requiredEquipment.equipmentId === userEquipment.equipmentId
                            )
                        ),
                    }
                    : prev
            );
        },
        [setActivityDetails]
    );

    const { handleToggleEquipment } = useActivityEquipment({
        activityId: activityDetails?.id ?? "",
        onQuantityUpdated,
    });

    return { handleToggleEquipment };
}
