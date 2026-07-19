import type { EquipmentListIconKey } from "@/features/equipmentList/utils/equipmentListIcons";

export interface EquipmentList {
    id: string;
    name: string;
    icon: string;
    items: string[];
    createdAt: string;
    updatedAt: string;
}

export interface CreateEquipmentListPayload {
    name: string;
    icon: EquipmentListIconKey;
    items: string[];
}

export type UpdateEquipmentListPayload = CreateEquipmentListPayload;
