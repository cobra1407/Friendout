import { useCallback, useEffect, useState } from "react";
import { isAxiosError } from "axios";
import { toast } from "sonner";
import {
    createEquipmentList,
    deleteEquipmentList,
    getEquipmentLists,
    updateEquipmentList
} from "@/features/equipmentList/api/equipmentList.api";
import type {
    CreateEquipmentListPayload,
    EquipmentList,
    UpdateEquipmentListPayload
} from "@/features/equipmentList/types/equipmentList.type";
import { getTranslation } from "@/i18n";

// The backend returns BadRequest(string) for validation failures (e.g. duplicate
// name) — surface that message directly instead of a generic one when available.
function extractErrorMessage(error: unknown, fallbackKey: string): string {
    if (isAxiosError(error) && typeof error.response?.data === "string") {
        return error.response.data;
    }
    return getTranslation(fallbackKey);
}

export function useEquipmentLists() {
    const [equipmentLists, setEquipmentLists] = useState<EquipmentList[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const fetchEquipmentLists = useCallback(async () => {
        try {
            setIsLoading(true);
            const lists = await getEquipmentLists();
            setEquipmentLists(lists);
        } catch (error) {
            toast.error(extractErrorMessage(error, "equipment_list.toast.fetch_error"));
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchEquipmentLists();
    }, [fetchEquipmentLists]);

    const handleCreate = useCallback(async (payload: CreateEquipmentListPayload) => {
        try {
            const created = await createEquipmentList(payload);
            setEquipmentLists((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
            toast.success(getTranslation("equipment_list.toast.create_success"));
            return created;
        } catch (error) {
            toast.error(extractErrorMessage(error, "equipment_list.toast.create_error"));
            return null;
        }
    }, []);

    const handleUpdate = useCallback(async (id: string, payload: UpdateEquipmentListPayload) => {
        try {
            const updated = await updateEquipmentList(id, payload);
            setEquipmentLists((prev) =>
                prev.map((list) => (list.id === id ? updated : list))
                    .sort((a, b) => a.name.localeCompare(b.name))
            );
            toast.success(getTranslation("equipment_list.toast.update_success"));
            return updated;
        } catch (error) {
            toast.error(extractErrorMessage(error, "equipment_list.toast.update_error"));
            return null;
        }
    }, []);

    const handleDelete = useCallback(async (id: string) => {
        try {
            await deleteEquipmentList(id);
            setEquipmentLists((prev) => prev.filter((list) => list.id !== id));
            toast.success(getTranslation("equipment_list.toast.delete_success"));
            return true;
        } catch (error) {
            toast.error(extractErrorMessage(error, "equipment_list.toast.delete_error"));
            return false;
        }
    }, []);

    return {
        equipmentLists,
        isLoading,
        createEquipmentList: handleCreate,
        updateEquipmentList: handleUpdate,
        deleteEquipmentList: handleDelete,
        refetch: fetchEquipmentLists
    };
}
