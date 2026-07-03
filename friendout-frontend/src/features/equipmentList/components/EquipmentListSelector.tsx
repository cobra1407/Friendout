import { useState } from "react";
import { toast } from "sonner";
import { ListChecks } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
    PopoverHeader,
    PopoverTitle,
    PopoverDescription
} from "@/components/ui/popover";
import { useEquipmentLists } from "@/features/equipmentList/hooks/useEquipmentLists";
import { getEquipmentListIcon } from "@/features/equipmentList/utils/equipmentListIcons";
import { getTranslation } from "@/i18n";

interface EquipmentListSelectorProps {
    /** Currently required equipment on the activity form, used to dedupe on merge. */
    currentItems: string[];
    onApply: (mergedItems: string[]) => void;
}

/**
 * Compact "prefill from a saved list" trigger, meant to be passed as
 * EquipmentManager's headerAction so it reads as part of the equipment
 * toolbar rather than a separate form field. Picking a list merges its
 * items into the current ones (case-insensitive dedupe) instead of
 * replacing them, so it composes with manually typed items or a second list.
 */
export function EquipmentListSelector({ currentItems, onApply }: EquipmentListSelectorProps) {
    const { equipmentLists, isLoading } = useEquipmentLists();
    const [open, setOpen] = useState(false);

    if (!isLoading && equipmentLists.length === 0) return null;

    const handleSelect = (listId: string) => {
        const list = equipmentLists.find((l) => l.id === listId);
        setOpen(false);
        if (!list) return;

        const existingLower = new Set(currentItems.map((item) => item.toLowerCase()));
        const newItems = list.items.filter((item) => !existingLower.has(item.toLowerCase()));

        if (newItems.length === 0) {
            toast.info(getTranslation("activity_form.equipment_list_selector.nothing_new"));
            return;
        }

        onApply([...currentItems, ...newItems]);
        toast.success(getTranslation("activity_form.equipment_list_selector.applied", { name: list.name }));
    };

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button type="button" variant="outline" size="sm" disabled={isLoading} className="shrink-0">
                    <ListChecks className="w-3.5 h-3.5" />
                    {getTranslation("activity_form.equipment_list_selector.trigger")}
                </Button>
            </PopoverTrigger>
            <PopoverContent align="end" className="w-64 p-2">
                <PopoverHeader className="px-2 pt-1 pb-2">
                    <PopoverTitle className="text-sm">
                        {getTranslation("activity_form.equipment_list_selector.label")}
                    </PopoverTitle>
                    <PopoverDescription className="text-xs">
                        {getTranslation("activity_form.equipment_list_selector.description")}
                    </PopoverDescription>
                </PopoverHeader>

                <div className="flex flex-col gap-0.5 max-h-64 overflow-y-auto">
                    {equipmentLists.map((list) => {
                        const Icon = getEquipmentListIcon(list.icon);
                        return (
                            <button
                                key={list.id}
                                type="button"
                                onClick={() => handleSelect(list.id)}
                                className="flex items-center gap-2 rounded-md px-2 py-1.5 text-sm text-left hover:bg-accent hover:text-accent-foreground transition-colors"
                            >
                                <Icon className="w-3.5 h-3.5 text-muted-foreground shrink-0" />
                                <span className="truncate flex-1">{list.name}</span>
                                <span className="text-xs text-muted-foreground shrink-0">
                                    {list.items.length}
                                </span>
                            </button>
                        );
                    })}
                </div>
            </PopoverContent>
        </Popover>
    );
}
