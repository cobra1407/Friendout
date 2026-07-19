import { Badge } from "@/components/ui/badge";
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal";
import { getEquipmentListIcon, getEquipmentListIconColorClasses } from "@/features/equipmentList/utils/equipmentListIcons";
import type { EquipmentList } from "@/features/equipmentList/types/equipmentList.type";
import { getTranslation } from "@/i18n";

interface EquipmentListViewModalProps {
    list: EquipmentList | null;
    onClose: () => void;
}

/**
 * Read-only modal showing the full item list of an EquipmentList.
 * Opened by clicking an EquipmentListCard; no edit/delete actions here,
 * those stay on the card itself.
 */
export function EquipmentListViewModal({ list, onClose }: EquipmentListViewModalProps) {
    if (!list) return null;

    const Icon = getEquipmentListIcon(list.icon);
    const colors = getEquipmentListIconColorClasses(list.icon);

    return (
        <Modal open={!!list} onClose={onClose} className="max-w-md">
            <ModalHeader>
                <div className="flex items-center gap-3">
                    <div className={`flex items-center justify-center w-9 h-9 rounded-full shrink-0 ${colors.bg} ${colors.icon}`}>
                        <Icon className="w-[18px] h-[18px]" />
                    </div>
                    <ModalTitle>{list.name}</ModalTitle>
                </div>
                <ModalDescription>
                    {getTranslation("equipment_list.view_modal.description")}
                </ModalDescription>
            </ModalHeader>

            {list.items.length > 0 ? (
                <div className="flex flex-wrap gap-1.5 max-h-80 overflow-y-auto">
                    {list.items.map((item) => (
                        <Badge key={item} variant="secondary" className="font-normal">
                            {item}
                        </Badge>
                    ))}
                </div>
            ) : (
                <p className="text-sm text-muted-foreground">
                    {getTranslation("equipment_list.empty_items")}
                </p>
            )}
        </Modal>
    );
}
