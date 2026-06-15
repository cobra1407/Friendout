import { Button } from "@/components/ui/button";
import { Modal, ModalDescription, ModalHeader, ModalTitle } from "@/components/ui/modal";
import { getTranslation } from "@/i18n";

interface EditConfirmationModalProps {
    showConfirmModal: boolean;
    isLoading: boolean;
    handleCancelUpdate: () => void;
    handleConfirmUpdate: () => void;
}

export const EditConfirmationModal = ({ showConfirmModal, isLoading, handleCancelUpdate, handleConfirmUpdate }: EditConfirmationModalProps) => {
    return (
        <Modal
            open={showConfirmModal}
            onClose={handleCancelUpdate}
            className="max-w-sm"
        >
            <ModalHeader>
                <ModalTitle>
                    {getTranslation("activity_form.confirm_update_modal.title")}
                </ModalTitle>
                <ModalDescription>
                    {getTranslation("activity_form.confirm_update_modal.description")}
                </ModalDescription>
            </ModalHeader>
            <div className="flex justify-end gap-2 mt-4">
                <Button
                    variant="outline"
                    onClick={handleCancelUpdate}
                    disabled={isLoading}
                >
                    {getTranslation("activity_form.confirm_update_modal.button_cancel")}
                </Button>
                <Button
                    onClick={handleConfirmUpdate}
                    disabled={isLoading}
                >
                    {isLoading
                        ? getTranslation("activity_form.button_saving")
                        : getTranslation("activity_form.confirm_update_modal.button_confirm")}
                </Button>
            </div>
        </Modal>
    );
};
