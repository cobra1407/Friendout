import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import type { ActivityFilter } from "@/features/activity/types/activityFilter.type";
import { getTranslation } from "@/i18n";

interface Props {
    value: ActivityFilter;
    onChange: (value: ActivityFilter) => void;
}

export const ActivityFilters = ({ value, onChange }: Props) => {
    // On transforme la valeur sélectionnée du select en ActivityFilter
    const mapSelectValueToFilter = (val: string): ActivityFilter => {
        switch (val) {
            case "all":
                return { timeFilter: "all", onlyOwnActivity: false };
            case "upcoming":
                return { timeFilter: "upcoming", onlyOwnActivity: false };
            case "past":
                return { timeFilter: "past", onlyOwnActivity: false };
            case "mine":
                return { timeFilter: "all", onlyOwnActivity: true };
            default:
                return { timeFilter: "all", onlyOwnActivity: false };
        }
    };


    // Inverse : pour que le select affiche correctement la valeur actuelle
    const mapFilterToSelectValue = (filter: ActivityFilter) => {
        if (filter.onlyOwnActivity) return "mine";
        return filter.timeFilter;
    };

    return (
        <Select
            value={mapFilterToSelectValue(value)}
            onValueChange={(val) => onChange(mapSelectValueToFilter(val))}
        >
            <SelectTrigger className="w-full sm:w-48">
                <SelectValue placeholder={getTranslation('activity.filter_placeholder')} />
            </SelectTrigger>
            <SelectContent>
                <SelectItem value="all">{getTranslation('activity.filter_all')}</SelectItem>
                <SelectItem value="upcoming">{getTranslation('activity.filter_upcoming')}</SelectItem>
                <SelectItem value="past">{getTranslation('activity.filter_past')}</SelectItem>
                <SelectItem value="mine">{getTranslation('activity.filter_mine')}</SelectItem>
            </SelectContent>
        </Select>
    );
};
