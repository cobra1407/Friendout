import { useState } from "react";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { Check, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
import type { ActivityFilter } from "@/features/activity/types/activityFilter.type";
import { getTranslation } from "@/i18n";

const options = [
    { value: "all", label: () => getTranslation('activity.filter_all') },
    { value: "upcoming", label: () => getTranslation('activity.filter_upcoming') },
    { value: "past", label: () => getTranslation('activity.filter_past') },
    { value: "mine", label: () => getTranslation('activity.filter_mine') },
];

interface Props {
    value: ActivityFilter;
    onChange: (value: ActivityFilter) => void;
}

export const ActivityFilters = ({ value, onChange }: Props) => {
    const [open, setOpen] = useState(false);

    const mapFilterToSelectValue = (filter: ActivityFilter) => {
        if (filter.onlyOwnActivity) return "mine";
        return filter.timeFilter;
    };

    const mapSelectValueToFilter = (val: string): ActivityFilter => {
        switch (val) {
            case "upcoming": return { timeFilter: "upcoming", onlyOwnActivity: false };
            case "past": return { timeFilter: "past", onlyOwnActivity: false };
            case "mine": return { timeFilter: "all", onlyOwnActivity: true };
            default: return { timeFilter: "all", onlyOwnActivity: false };
        }
    };

    const current = mapFilterToSelectValue(value);
    const currentLabel = options.find(o => o.value === current)?.label() ?? getTranslation('activity.filter_placeholder');

    return (
        <Popover modal={false} open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    className="w-full h-10 sm:w-48 justify-between border font-normal"
                >
                    {currentLabel}
                    <ChevronDown className="h-4 w-4 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-48 p-1" align="start">
                {options.map((option) => (
                    <div
                        key={option.value}
                        className={cn(
                            "relative flex cursor-default select-none items-center rounded-sm py-1.5 pl-8 pr-2 text-sm outline-none hover:bg-accent hover:text-accent-foreground cursor-pointer"
                        )}
                        onClick={() => {
                            onChange(mapSelectValueToFilter(option.value));
                            setOpen(false);
                        }}
                    >
                        {current === option.value && (
                            <span className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
                                <Check className="h-4 w-4" />
                            </span>
                        )}
                        {option.label()}
                    </div>
                ))}
            </PopoverContent>
        </Popover>
    );
};
