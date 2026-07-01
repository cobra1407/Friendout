import type {
    CreateEquipmentListPayload,
    EquipmentList,
    UpdateEquipmentListPayload
} from "@/features/equipmentList/types/equipmentList.type";
import api from "@/lib/api/api";

export async function getEquipmentLists(): Promise<EquipmentList[]> {
    const response = await api.get<EquipmentList[]>("/equipment-lists");
    return response.data;
}

export async function getEquipmentListById(id: string): Promise<EquipmentList> {
    const response = await api.get<EquipmentList>(`/equipment-lists/${id}`);
    return response.data;
}

export async function createEquipmentList(payload: CreateEquipmentListPayload): Promise<EquipmentList> {
    const response = await api.post<EquipmentList>("/equipment-lists", payload);
    return response.data;
}

export async function updateEquipmentList(id: string, payload: UpdateEquipmentListPayload): Promise<EquipmentList> {
    const response = await api.put<EquipmentList>(`/equipment-lists/${id}`, payload);
    return response.data;
}

export async function deleteEquipmentList(id: string): Promise<void> {
    await api.delete(`/equipment-lists/${id}`);
}
