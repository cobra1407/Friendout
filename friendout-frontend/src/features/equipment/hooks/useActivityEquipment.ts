import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { getTranslation } from "@/i18n";
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
    const mutation = useMutation({
        mutationFn: ({ equipmentId, quantity }: { equipmentId: string; quantity: number }) =>
            updateUserEquipmentQuantity({ equipmentId, activityId, quantity }),
        onSuccess: onQuantityUpdated,
        onError: () => toast.error(getTranslation("equipment.update_error")),
    });

    const handleToggleEquipment = (equipmentId: string, quantity: number) =>
        mutation.mutate({ equipmentId, quantity });

    return { handleToggleEquipment, isUpdatingEquipment: mutation.isPending };
}
