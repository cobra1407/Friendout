import { Button } from "@/components/ui/button";
import { getTranslation } from "@/i18n";
import { Plus } from "lucide-react";

interface AddSubActivityButtonProps {
    onClick: () => void;
    className?: string;
}

const AddSubActivityButton = ({ onClick, className }: AddSubActivityButtonProps) => {
    return (
        <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={onClick}
            className={`bg-blue-100 text-blue-800 ${className || ''}`}
        >
            <Plus className="w-4 h-4 mr-2" />
            {getTranslation("sub_activity.new_subactivity")}
        </Button>
    );
}

export default AddSubActivityButton;
