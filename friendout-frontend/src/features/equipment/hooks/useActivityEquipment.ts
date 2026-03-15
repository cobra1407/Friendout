import { useCallback } from "react";
import { toast } from "sonner";
import { updateUserEquipmentQuantity } from "@/features/equipment/api/equipment.api";
import type { UserEquipment } from "@/features/equipment/types/userEquipment";

export interface UseActivityEquipmentParams {
    activityId: string;
    onQuantityUpdated: (userEquipments: UserEquipment[]) => void;
}

export function useActivityEquipment({
    activityId,
    onQuantityUpdated,
}: UseActivityEquipmentParams) {
    const handleToggleEquipment = useCallback(
        async (equipmentId: string, quantity: number) => {
            try {
                const updated = await updateUserEquipmentQuantity({
                    equipmentId,
                    activityId,
                    quantity,
                });
                onQuantityUpdated(updated);
            } catch {
                toast.error("Erreur lors de la modification de l'équipement");
            }
        },
        [activityId, onQuantityUpdated]
    );

    return { handleToggleEquipment };
}
