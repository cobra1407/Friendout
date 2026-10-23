import type { LucideProps } from "lucide-react";
import {
    DEFAULT_EQUIPMENT_LIST_ICON,
    EQUIPMENT_LIST_ICONS,
    EQUIPMENT_LIST_ICON_KEYS,
    type EquipmentListIconKey,
} from "@/features/equipmentList/utils/equipmentListIcons";

interface EquipmentListIconProps extends LucideProps {
    /** Icon key stored on the list. Unknown/legacy keys fall back to the default icon. */
    iconKey: string;
}

const isKnownIconKey = (key: string): key is EquipmentListIconKey =>
    (EQUIPMENT_LIST_ICON_KEYS as readonly string[]).includes(key);

/**
 * Renders the Lucide icon matching an equipment list's icon key.
 *
 * The icon is looked up in a static map instead of being returned by a function call, so React
 * never sees a component "created during render" (which would remount it on every render).
 */
export function EquipmentListIcon({ iconKey, ...props }: EquipmentListIconProps) {
    const Icon = EQUIPMENT_LIST_ICONS[isKnownIconKey(iconKey) ? iconKey : DEFAULT_EQUIPMENT_LIST_ICON];
    return <Icon {...props} />;
}
