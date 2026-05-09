import { getTranslation } from "@/i18n";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";

interface CreateActivityButtonProps {
    onCreateActivity: () => void;
    className?: string;
}

const CreateActivityButton = ({ onCreateActivity, className }: CreateActivityButtonProps) => {
    return (
        <>
            <Button
                className={className || "flex items-center gap-2 h-9 px-3 sm:px-4 cursor-pointer"}
                onClick={onCreateActivity}
            >
                <Plus className="w-4 h-4" />
                <span className="hidden sm:inline text-sm">
                    {getTranslation('header.create_activity')}
                </span>
            </Button>
        </>
    );
};

export default CreateActivityButton;
