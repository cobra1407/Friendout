import { Button } from "@/components/ui/button";
import { Modal, ModalDescription, ModalHeader, ModalTitle } from "@/components/ui/modal";
import { getTranslation } from "@/i18n";

interface DeleteEquipmentListModalProps {
    open: boolean;
    listName: string;
    isDeleting: boolean;
    onCancel: () => void;
    onConfirm: () => void;
}

export function DeleteEquipmentListModal({
    open,
    listName,
    isDeleting,
    onCancel,
    onConfirm
}: DeleteEquipmentListModalProps) {
    return (
        <Modal open={open} onClose={onCancel} className="max-w-sm">
            <ModalHeader>
                <ModalTitle>
                    {getTranslation("equipment_list.delete_confirm.title", { name: listName })}
                </ModalTitle>
                <ModalDescription>
                    {getTranslation("equipment_list.delete_confirm.description")}
                </ModalDescription>
            </ModalHeader>
            <div className="flex justify-end gap-2 mt-4">
                <Button variant="outline" onClick={onCancel} disabled={isDeleting}>
                    {getTranslation("common.cancel")}
                </Button>
                <Button variant="destructive" onClick={onConfirm} disabled={isDeleting}>
                    {getTranslation("equipment_list.delete_confirm.confirm_button")}
                </Button>
            </div>
        </Modal>
    );
}
