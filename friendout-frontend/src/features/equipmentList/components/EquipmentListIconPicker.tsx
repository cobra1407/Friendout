import { cn } from "@/lib/utils";
import { Label } from "@/components/ui/label";
import {
    EQUIPMENT_LIST_ICON_KEYS,
    getEquipmentListIcon,
    getEquipmentListIconColorClasses,
    type EquipmentListIconKey
} from "@/features/equipmentList/utils/equipmentListIcons";
import { getTranslation } from "@/i18n";

interface EquipmentListIconPickerProps {
    value: string;
    onChange: (icon: EquipmentListIconKey) => void;
}

export function EquipmentListIconPicker({ value, onChange }: EquipmentListIconPickerProps) {
    return (
        <div className="space-y-2">
            <Label>{getTranslation("equipment_list.form.icon_label")}</Label>
            <div className="grid grid-cols-8 gap-2" role="radiogroup" aria-label={getTranslation("equipment_list.form.icon_label")}>
                {EQUIPMENT_LIST_ICON_KEYS.map((key) => {
                    const Icon = getEquipmentListIcon(key);
                    const colors = getEquipmentListIconColorClasses(key);
                    const isSelected = value === key;

                    return (
                        <button
                            key={key}
                            type="button"
                            role="radio"
                            aria-checked={isSelected}
                            onClick={() => onChange(key)}
                            className={cn(
                                "flex items-center justify-center w-9 h-9 rounded-full transition-all",
                                colors.bg,
                                colors.icon,
                                isSelected
                                    ? "ring-2 ring-offset-2 ring-offset-background ring-current"
                                    : "opacity-60 hover:opacity-100"
                            )}
                        >
                            <Icon className="w-4 h-4" />
                        </button>
                    );
                })}
            </div>
        </div>
    );
}
