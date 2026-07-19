import {
    Backpack,
    Tent,
    Mountain,
    Bike,
    Footprints,
    Waves,
    Snowflake,
    Utensils,
    Music,
    Camera,
    Dumbbell,
    Gamepad2,
    Palette,
    BookOpen,
    Package,
    type LucideIcon
} from "lucide-react";

/**
 * Whitelist of icon keys an equipment list can use. Must stay in sync with
 * EquipmentListIcons.AllowedKeys on the backend (Friendout.Domain/Constants).
 */
export const EQUIPMENT_LIST_ICON_KEYS = [
    "backpack",
    "tent",
    "mountain",
    "bike",
    "footprints",
    "waves",
    "snowflake",
    "utensils",
    "music",
    "camera",
    "dumbbell",
    "gamepad-2",
    "palette",
    "book-open",
    "package"
] as const;

export type EquipmentListIconKey = typeof EQUIPMENT_LIST_ICON_KEYS[number];

export const DEFAULT_EQUIPMENT_LIST_ICON: EquipmentListIconKey = "backpack";

const ICON_COMPONENTS: Record<EquipmentListIconKey, LucideIcon> = {
    backpack: Backpack,
    tent: Tent,
    mountain: Mountain,
    bike: Bike,
    footprints: Footprints,
    waves: Waves,
    snowflake: Snowflake,
    utensils: Utensils,
    music: Music,
    camera: Camera,
    dumbbell: Dumbbell,
    "gamepad-2": Gamepad2,
    palette: Palette,
    "book-open": BookOpen,
    package: Package
};

/**
 * Resolves an icon key to its Lucide component, falling back to the default
 * icon for unknown/legacy keys instead of throwing.
 */
export function getEquipmentListIcon(key: string): LucideIcon {
    return ICON_COMPONENTS[key as EquipmentListIconKey] ?? ICON_COMPONENTS[DEFAULT_EQUIPMENT_LIST_ICON];
}

interface EquipmentListIconColorClasses {
    /** Icon glyph color. */
    icon: string;
    /** Soft tinted background, for icon badges/circles. */
    bg: string;
    /** Solid fill, for accents like the card's left spine. */
    solid: string;
}

// Full literal Tailwind classes (not built via template strings) so the JIT
// compiler picks them up. Colors are chosen to loosely evoke each icon's
// theme (green for camping, blue for snow, etc.) so lists are easier to spot
// at a glance rather than all sharing the app's single accent color.
const ICON_COLOR_CLASSES: Record<EquipmentListIconKey, EquipmentListIconColorClasses> = {
    backpack: { icon: "text-amber-600 dark:text-amber-400", bg: "bg-amber-100 dark:bg-amber-950/40", solid: "bg-amber-500" },
    tent: { icon: "text-green-600 dark:text-green-400", bg: "bg-green-100 dark:bg-green-950/40", solid: "bg-green-500" },
    mountain: { icon: "text-stone-600 dark:text-stone-400", bg: "bg-stone-100 dark:bg-stone-950/40", solid: "bg-stone-500" },
    bike: { icon: "text-cyan-600 dark:text-cyan-400", bg: "bg-cyan-100 dark:bg-cyan-950/40", solid: "bg-cyan-500" },
    footprints: { icon: "text-orange-600 dark:text-orange-400", bg: "bg-orange-100 dark:bg-orange-950/40", solid: "bg-orange-500" },
    waves: { icon: "text-sky-600 dark:text-sky-400", bg: "bg-sky-100 dark:bg-sky-950/40", solid: "bg-sky-500" },
    snowflake: { icon: "text-blue-600 dark:text-blue-400", bg: "bg-blue-100 dark:bg-blue-950/40", solid: "bg-blue-500" },
    utensils: { icon: "text-red-600 dark:text-red-400", bg: "bg-red-100 dark:bg-red-950/40", solid: "bg-red-500" },
    music: { icon: "text-purple-600 dark:text-purple-400", bg: "bg-purple-100 dark:bg-purple-950/40", solid: "bg-purple-500" },
    camera: { icon: "text-indigo-600 dark:text-indigo-400", bg: "bg-indigo-100 dark:bg-indigo-950/40", solid: "bg-indigo-500" },
    dumbbell: { icon: "text-rose-600 dark:text-rose-400", bg: "bg-rose-100 dark:bg-rose-950/40", solid: "bg-rose-500" },
    "gamepad-2": { icon: "text-violet-600 dark:text-violet-400", bg: "bg-violet-100 dark:bg-violet-950/40", solid: "bg-violet-500" },
    palette: { icon: "text-pink-600 dark:text-pink-400", bg: "bg-pink-100 dark:bg-pink-950/40", solid: "bg-pink-500" },
    "book-open": { icon: "text-teal-600 dark:text-teal-400", bg: "bg-teal-100 dark:bg-teal-950/40", solid: "bg-teal-500" },
    package: { icon: "text-slate-600 dark:text-slate-400", bg: "bg-slate-100 dark:bg-slate-950/40", solid: "bg-slate-500" }
};

/**
 * Resolves an icon key to its themed color classes, falling back to the
 * default icon's colors for unknown/legacy keys.
 */
export function getEquipmentListIconColorClasses(key: string): EquipmentListIconColorClasses {
    return ICON_COLOR_CLASSES[key as EquipmentListIconKey] ?? ICON_COLOR_CLASSES[DEFAULT_EQUIPMENT_LIST_ICON];
}
