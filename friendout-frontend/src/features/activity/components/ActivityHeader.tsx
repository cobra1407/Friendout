import { Button } from '@/components/ui/button';
import { ArrowLeft, Share2, Edit, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { getTranslation } from '@/i18n';

type ActivityHeaderModel = {
    id: string;
    title: string;
    description: string;
    createdBy: string;
};

interface ActivityHeaderProps {
    activity: ActivityHeaderModel;
    currentUserId?: string;
    onBack: () => void;
    onEdit?: (activity: ActivityHeaderModel) => void;
    onDelete?: (activityId: string) => void;
}

export default function ActivityHeader({ activity, currentUserId, onBack, onEdit, onDelete }: ActivityHeaderProps) {
    const handleShare = () => {
        if (navigator.share) {
            navigator.share({
                title: activity.title,
                text: activity.description,
                url: window.location.href,
            });
        } else {
            navigator.clipboard.writeText(window.location.href);
            toast.success(getTranslation('activity.link_copied'));
        }
    };
    return (
        <header className="bg-white shadow-sm border-b">
            <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center justify-between h-16">
                    {/* Bouton retour + titre */}
                    <div className="flex items-center gap-2">
                        <Button
                            variant="ghost"
                            onClick={onBack}
                            className="flex items-center gap-2 px-2 sm:px-3"
                        >
                            <ArrowLeft className="w-4 h-4" />
                            <span className="hidden sm:inline">{getTranslation('common.back')}</span>
                        </Button>
                        <h1 className="text-xl font-semibold line-clamp-2">
                            {activity.title}
                        </h1>
                    </div>

                    {/* Actions */}
                    <div className="flex items-center gap-2">
                        <Button
                            variant="outline"
                            onClick={handleShare}
                            className="flex items-center gap-2 px-2 sm:px-3"
                        >
                            <Share2 className="w-4 h-4" />
                            <span className="hidden sm:inline">{getTranslation('common.share')}</span>
                        </Button>

                        {currentUserId === activity.createdBy && (
                            <>
                                <Button
                                    variant="outline"
                                    onClick={() => onEdit?.(activity)}
                                    className="flex items-center gap-2 px-2 sm:px-3"
                                >
                                    <Edit className="w-4 h-4" />
                                    <span className="hidden sm:inline">{getTranslation('common.edit')}</span>
                                </Button>

                                <Button
                                    variant="destructive"
                                    onClick={() => {
                                        if (confirm(getTranslation('activity.confirm_delete'))) {
                                            onDelete?.(activity.id);
                                        }
                                    }}
                                    className="flex items-center gap-2 px-2 sm:px-3"
                                >
                                    <Trash2 className="w-4 h-4" />
                                    <span className="hidden sm:inline">{getTranslation('common.delete')}</span>
                                </Button>
                            </>
                        )}
                    </div>
                </div>
            </div>
        </header>
    );
}
