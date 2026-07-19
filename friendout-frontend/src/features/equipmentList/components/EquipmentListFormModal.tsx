import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Modal, ModalDescription, ModalHeader, ModalTitle } from "@/components/ui/modal";
import EquipmentManager from "@/features/equipment/component/EquipmentManager";
import { EquipmentListIconPicker } from "@/features/equipmentList/components/EquipmentListIconPicker";
import type { EquipmentList } from "@/features/equipmentList/types/equipmentList.type";
import { DEFAULT_EQUIPMENT_LIST_ICON, type EquipmentListIconKey } from "@/features/equipmentList/utils/equipmentListIcons";
import { getTranslation } from "@/i18n";

interface EquipmentListFormModalProps {
    open: boolean;
    isSubmitting: boolean;
    /** When provided, the modal edits this list instead of creating a new one. */
    initialList?: EquipmentList;
    onClose: () => void;
    onSubmit: (payload: { name: string; icon: EquipmentListIconKey; items: string[] }) => Promise<boolean>;
}

export function EquipmentListFormModal({
    open,
    isSubmitting,
    initialList,
    onClose,
    onSubmit
}: EquipmentListFormModalProps) {
    const [name, setName] = useState(initialList?.name ?? "");
    const [icon, setIcon] = useState<EquipmentListIconKey>(
        (initialList?.icon as EquipmentListIconKey) ?? DEFAULT_EQUIPMENT_LIST_ICON
    );
    const [items, setItems] = useState<string[]>(initialList?.items ?? []);
    const isEditMode = !!initialList;

    // No reset effect here on purpose: the parent remounts this component (via a
    // `key` tied to the target list + open state) every time the modal opens, so
    // the state above is already correct on the very first render. Resetting via
    // an effect after mount was the previous approach, but it rendered one frame
    // with stale values before correcting itself, causing a visible icon flash.

    const handleSubmit = async () => {
        const trimmedName = name.trim();
        if (!trimmedName) return;

        const success = await onSubmit({ name: trimmedName, icon, items });
        if (success) onClose();
    };

    // Enter anywhere in the form saves the list — except while the user is actively
    // typing into one of EquipmentManager's own inputs (add/edit item), since that
    // field's non-empty value signals intent to add/edit an item, not to save.
    const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
        if (e.key !== "Enter") return;

        const target = e.target as HTMLInputElement;
        if (target.dataset?.equipmentManagerInput && target.value.trim()) return;

        e.preventDefault();
        handleSubmit();
    };

    return (
        <Modal open={open} onClose={onClose} className="max-w-lg max-h-[85vh] flex flex-col p-0 gap-0 overflow-hidden">
            <div className="px-6 pt-6 pb-4" onKeyDown={handleKeyDown}>
                <ModalHeader>
                    <ModalTitle>
                        {isEditMode
                            ? getTranslation("equipment_list.form.edit_title")
                            : getTranslation("equipment_list.form.create_title")}
                    </ModalTitle>
                    <ModalDescription>
                        {getTranslation("equipment_list.form.description")}
                    </ModalDescription>
                </ModalHeader>
            </div>

            <div onKeyDown={handleKeyDown} className="flex-1 min-h-0 overflow-y-auto px-6">
                <div className="space-y-4 pb-4">
                    <div className="space-y-2">
                        <Label htmlFor="equipment-list-name">
                            {getTranslation("equipment_list.form.name_label")}
                        </Label>
                        <Input
                            id="equipment-list-name"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            placeholder={getTranslation("equipment_list.form.name_placeholder")}
                            autoFocus
                        />
                    </div>

                    <EquipmentListIconPicker value={icon} onChange={setIcon} />

                    <EquipmentManager equipment={items} onChange={setItems} />
                </div>
            </div>

            <div className="flex justify-end gap-2 px-6 py-4 border-t">
                <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
                    {getTranslation("common.cancel")}
                </Button>
                <Button onClick={handleSubmit} disabled={isSubmitting || !name.trim()}>
                    {isSubmitting
                        ? getTranslation("common.saving")
                        : getTranslation("common.save")}
                </Button>
            </div>
        </Modal>
    );
}
