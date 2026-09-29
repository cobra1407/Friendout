import { useCallback } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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

const EQUIPMENT_LISTS_KEY = ["equipmentLists", "me"] as const;

// The backend returns BadRequest(string) for validation failures (e.g. duplicate
// name) — surface that message directly instead of a generic one when available.
function extractErrorMessage(error: unknown, fallbackKey: string): string {
    if (isAxiosError(error) && typeof error.response?.data === "string") {
        return error.response.data;
    }
    return getTranslation(fallbackKey);
}

const sortByName = (lists: EquipmentList[]) =>
    [...lists].sort((a, b) => a.name.localeCompare(b.name));

export function useEquipmentLists() {
    const qc = useQueryClient();

    const { data: equipmentLists = [], isLoading } = useQuery({
        queryKey: EQUIPMENT_LISTS_KEY,
        queryFn: getEquipmentLists,
    });

    const createMutation = useMutation({
        mutationFn: (payload: CreateEquipmentListPayload) => createEquipmentList(payload),
        onSuccess: (created) => {
            qc.setQueryData<EquipmentList[]>(EQUIPMENT_LISTS_KEY, (prev = []) =>
                sortByName([...prev, created])
            );
            toast.success(getTranslation("equipment_list.toast.create_success"));
        },
        onError: (error) => toast.error(extractErrorMessage(error, "equipment_list.toast.create_error")),
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, payload }: { id: string; payload: UpdateEquipmentListPayload }) =>
            updateEquipmentList(id, payload),
        onSuccess: (updated) => {
            qc.setQueryData<EquipmentList[]>(EQUIPMENT_LISTS_KEY, (prev = []) =>
                sortByName(prev.map((list) => (list.id === updated.id ? updated : list)))
            );
            toast.success(getTranslation("equipment_list.toast.update_success"));
        },
        onError: (error) => toast.error(extractErrorMessage(error, "equipment_list.toast.update_error")),
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) => deleteEquipmentList(id),
        onSuccess: (_data, id) => {
            qc.setQueryData<EquipmentList[]>(EQUIPMENT_LISTS_KEY, (prev = []) =>
                prev.filter((list) => list.id !== id)
            );
            toast.success(getTranslation("equipment_list.toast.delete_success"));
        },
        onError: (error) => toast.error(extractErrorMessage(error, "equipment_list.toast.delete_error")),
    });

    const handleCreate = useCallback(
        async (payload: CreateEquipmentListPayload) => {
            try {
                return await createMutation.mutateAsync(payload);
            } catch {
                return null;
            }
        },
        [createMutation]
    );

    const handleUpdate = useCallback(
        async (id: string, payload: UpdateEquipmentListPayload) => {
            try {
                return await updateMutation.mutateAsync({ id, payload });
            } catch {
                return null;
            }
        },
        [updateMutation]
    );

    const handleDelete = useCallback(
        async (id: string) => {
            try {
                await deleteMutation.mutateAsync(id);
                return true;
            } catch {
                return false;
            }
        },
        [deleteMutation]
    );

    return {
        equipmentLists,
        isLoading,
        createEquipmentList: handleCreate,
        updateEquipmentList: handleUpdate,
        deleteEquipmentList: handleDelete,
        refetch: () => qc.invalidateQueries({ queryKey: EQUIPMENT_LISTS_KEY }),
    };
}
