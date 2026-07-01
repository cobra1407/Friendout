export interface EquipmentList {
    id: string;
    name: string;
    items: string[];
    createdAt: string;
    updatedAt: string;
}

export interface CreateEquipmentListPayload {
    name: string;
    items: string[];
}

export type UpdateEquipmentListPayload = CreateEquipmentListPayload;
