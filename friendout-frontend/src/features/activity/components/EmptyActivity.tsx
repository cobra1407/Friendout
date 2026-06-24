import { Plus } from "lucide-react";
import { getTranslation } from "@/i18n";

/**
 * A component to display when no activities are found.
 *
 * @returns {JSX.Element} A React component displaying a "no activities found" message.
 */
const EmptyActivity = () => {
    return (
        <div className="text-center py-12">
            <div className="text-muted-foreground mb-4">
                <Plus className="w-16 h-16 mx-auto" />
            </div>
            <h3 className="text-lg font-medium text-foreground mb-2">
                {getTranslation('activity.no_activity_found')}
            </h3>
            <p className="text-muted-foreground mb-6">{getTranslation('activity.try_modify_search')}</p>
        </div>
    );
};

export default EmptyActivity;
