import type { Equipment } from "@/features/equipment/types/equipment.type";
import type { SetEquipment } from "@/features/equipment/types/setEquipment.type";
import api from "@/lib/api/api";
import type { UserEquipment } from "../types/userEquipment";

export async function getActivityUserEquipments(activityId: string) : Promise<Equipment[]> {
  const response = await api.get<Equipment[]>(`/activities/${activityId}/user/equipment`);
  return response.data;
}

export async function updateUserEquipmentQuantity(
  payload: SetEquipment
): Promise<UserEquipment[]> {
  const response = await api.put<UserEquipment[]>(
    `/activities/${payload.activityId}/user/equipment`,
    {
      equipmentId: payload.equipmentId,
      quantity: payload.quantity
    }
  );

  return response.data;
}
