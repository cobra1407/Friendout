import { Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { getTranslation } from "@/i18n";

interface Props {
    value: string;
    onChange: (value: string) => void;
}

export const ActivitySearch = ({ value, onChange }: Props) => {
    return (
        <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-4 h-4" />
            <Input
                placeholder={getTranslation('activity.search_placeholder')}
                value={value}
                onChange={(e) => onChange(e.target.value)}
                className="pl-10"
            />
        </div>
    );
};
