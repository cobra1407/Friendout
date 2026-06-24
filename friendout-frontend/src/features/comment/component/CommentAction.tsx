import { Edit, Trash2 } from "lucide-react";
import { getTranslation } from "@/i18n";

type CommentActionsProps = {
    onEdit: () => void;
    onDelete: () => void;
};

export default function CommentActions({ onEdit, onDelete }: CommentActionsProps) {
    return (
        <div className="flex gap-2">
            <button
                type="button"
                onClick={onEdit}
                className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-accent rounded-full transition-colors cursor-pointer"
                title={getTranslation('comments.edit_title')}
            >
                <Edit className="h-4 w-4" />
            </button>

            <button
                type="button"
                onClick={onDelete}
                className="p-1.5 text-muted-foreground hover:text-destructive hover:bg-destructive/10 rounded-full transition-colors cursor-pointer"
                title={getTranslation('comments.delete_title')}
            >
                <Trash2 className="h-4 w-4" />
            </button>
        </div>
    );
}
