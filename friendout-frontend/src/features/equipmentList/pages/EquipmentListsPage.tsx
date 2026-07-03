import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";
import { ActivityLayout } from "@/features/activity/layout/activityLayout";
import { Header } from "@/components/header";
import { authApi } from "@/features/auth/api/auth.api";
import { Button } from "@/components/ui/button";
import { getTranslation } from "@/i18n";
import { useEquipmentLists } from "@/features/equipmentList/hooks/useEquipmentLists";
import { EquipmentListCard } from "@/features/equipmentList/components/EquipmentListCard";
import { EquipmentListFormModal } from "@/features/equipmentList/components/EquipmentListFormModal";
import { DeleteEquipmentListModal } from "@/features/equipmentList/components/DeleteEquipmentListModal";
import type { EquipmentList } from "@/features/equipmentList/types/equipmentList.type";
import type { EquipmentListIconKey } from "@/features/equipmentList/utils/equipmentListIcons";
import EquipmentListCardSkeleton from "../components/EquipmentListCardSkeleton";

export default function EquipmentListsPage() {
    const navigate = useNavigate();
    const {
        equipmentLists,
        isLoading,
        createEquipmentList,
        updateEquipmentList,
        deleteEquipmentList
    } = useEquipmentLists();

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [editingList, setEditingList] = useState<EquipmentList | undefined>(undefined);
    const [listPendingDeletion, setListPendingDeletion] = useState<EquipmentList | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const handleLogout = async () => {
        await authApi.logout();
        navigate("/login");
    };

    const openCreateModal = () => {
        setEditingList(undefined);
        setIsFormOpen(true);
    };

    const openEditModal = (list: EquipmentList) => {
        setEditingList(list);
        setIsFormOpen(true);
    };

    const handleFormSubmit = async (payload: { name: string; icon: EquipmentListIconKey; items: string[] }) => {
        setIsSubmitting(true);
        try {
            const result = editingList
                ? await updateEquipmentList(editingList.id, payload)
                : await createEquipmentList(payload);
            return result !== null;
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleConfirmDelete = async () => {
        if (!listPendingDeletion) return;
        setIsDeleting(true);
        try {
            const success = await deleteEquipmentList(listPendingDeletion.id);
            if (success) setListPendingDeletion(null);
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <ActivityLayout
            header={
                <Header
                    onCreateActivity={() => navigate("/activities/createActivity")}
                    onLogout={handleLogout}
                />
            }
        >
            <div className="max-w-7xl mx-auto w-full pb-10 space-y-6 px-4">
                <div className="pt-6 flex items-start justify-between gap-4">
                    <div>
                        <h1 className="text-xl font-bold text-foreground">
                            {getTranslation("equipment_list.page_title")}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {getTranslation("equipment_list.page_description")}
                        </p>
                    </div>
                    <Button onClick={openCreateModal} className="shrink-0">
                        <Plus className="w-4 h-4" />
                        {getTranslation("equipment_list.new_list")}
                    </Button>
                </div>
                {isLoading ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                        {
                            Array.from({ length: 6 }).map((_, i) => (
                                <EquipmentListCardSkeleton key={i} />
                            ))
                        }
                    </div>
                ) : equipmentLists.length === 0 ? (
                    <div className="flex flex-col items-center justify-center gap-3 py-16 text-center">
                        <p className="text-sm text-muted-foreground max-w-sm">
                            {getTranslation("equipment_list.empty_state")}
                        </p>
                        <Button variant="outline" onClick={openCreateModal}>
                            <Plus className="w-4 h-4" />
                            {getTranslation("equipment_list.new_list")}
                        </Button>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                        {equipmentLists.map((list) => (
                            <EquipmentListCard
                                key={list.id}
                                list={list}
                                onEdit={openEditModal}
                                onDelete={setListPendingDeletion}
                            />
                        ))}
                    </div>
                )}
            </div>

            <EquipmentListFormModal
                open={isFormOpen}
                isSubmitting={isSubmitting}
                initialList={editingList}
                onClose={() => setIsFormOpen(false)}
                onSubmit={handleFormSubmit}
            />

            <DeleteEquipmentListModal
                open={!!listPendingDeletion}
                listName={listPendingDeletion?.name ?? ""}
                isDeleting={isDeleting}
                onCancel={() => setListPendingDeletion(null)}
                onConfirm={handleConfirmDelete}
            />
        </ActivityLayout>
    );
}
