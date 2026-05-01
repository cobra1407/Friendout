import { Button } from '@/components/ui/button';
import { ArrowLeft, Share2, Edit, Trash2, Calendar, MoreHorizontal } from 'lucide-react';
import { toast } from 'sonner';
import { getTranslation } from '@/i18n';
import { downloadIcs, getGoogleCalendarUrl, getOutlookCalendarUrl } from '@/lib/utils/calendar.utils';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

type ActivityHeaderModel = {
    id: string;
    title: string;
    description: string;
    startAt: string;
    endAt?: string | null;
    location?: string | null;
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
        const copyToClipboard = (text: string) => {
            // navigator.clipboard requires HTTPS or localhost.
            // On plain HTTP (Docker without TLS), use the legacy execCommand fallback.
            if (navigator.clipboard) {
                navigator.clipboard.writeText(text)
                    .then(() => toast.success(getTranslation('activity.link_copied')))
                    .catch(() => execCommandCopy(text));
            } else {
                execCommandCopy(text);
            }
        };

        const execCommandCopy = (text: string) => {
            const textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            document.body.appendChild(textarea);
            textarea.focus();
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
            toast.success(getTranslation('activity.link_copied'));
        };

        const doShare = () => {
            if (navigator.share) {
                navigator.share({
                    title: activity.title,
                    text: activity.description,
                    url: window.location.href,
                }).catch((err) => {
                    if (err?.name !== 'AbortError') {
                        copyToClipboard(window.location.href);
                    }
                });
            } else {
                copyToClipboard(window.location.href);
            }
        };

        // Defer slightly so the dropdown has time to close before the
        // native share sheet opens — otherwise the user gesture is consumed
        // by the dropdown and navigator.share() may be blocked.
        setTimeout(doShare, 0);
    };

    const calendarEvent = {
        id: activity.id,
        title: activity.title,
        description: activity.description,
        startAt: activity.startAt,
        endAt: activity.endAt ?? null,
        location: activity.location ?? null,
    }

    const handleExportIcs = () => {
        try {
            downloadIcs(calendarEvent)
            toast.success(getTranslation('common.calendar_exported'))
        } catch {
            toast.error(getTranslation('common.export_error'))
        }
    }

    const handleExportGoogle = () => window.open(getGoogleCalendarUrl(calendarEvent), '_blank')
    const handleExportOutlook = () => window.open(getOutlookCalendarUrl(calendarEvent), '_blank')

    return (
        <header className="bg-white shadow-sm border-b">
            <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center justify-between h-16">
                    {/* Title + Back button */}
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

                        {/* ── Mobile : everything in a single ⋯ dropdown ── */}
                        <div className="flex sm:hidden">
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="outline" size="icon">
                                        <MoreHorizontal className="w-4 h-4" />
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end">
                                    <DropdownMenuItem onClick={handleShare}>
                                        <Share2 className="w-4 h-4 mr-2" />
                                        {getTranslation('common.share')}
                                    </DropdownMenuItem>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem onClick={handleExportGoogle}>
                                        <Calendar className="w-4 h-4 mr-2" />
                                        Google Calendar
                                    </DropdownMenuItem>
                                    <DropdownMenuItem onClick={handleExportOutlook}>
                                        <Calendar className="w-4 h-4 mr-2" />
                                        Outlook
                                    </DropdownMenuItem>
                                    <DropdownMenuItem onClick={handleExportIcs}>
                                        <Calendar className="w-4 h-4 mr-2" />
                                        {getTranslation('common.export_ics')}
                                    </DropdownMenuItem>
                                    {currentUserId === activity.createdBy && (
                                        <>
                                            <DropdownMenuSeparator />
                                            <DropdownMenuItem onClick={() => onEdit?.(activity)}>
                                                <Edit className="w-4 h-4 mr-2" />
                                                {getTranslation('common.edit')}
                                            </DropdownMenuItem>
                                            <DropdownMenuItem
                                                className="text-destructive focus:text-destructive"
                                                onClick={() => {
                                                    if (confirm(getTranslation('activity.confirm_delete'))) {
                                                        onDelete?.(activity.id);
                                                    }
                                                }}
                                            >
                                                <Trash2 className="w-4 h-4 mr-2" />
                                                {getTranslation('common.delete')}
                                            </DropdownMenuItem>
                                        </>
                                    )}
                                </DropdownMenuContent>
                            </DropdownMenu>
                        </div>

                        {/* ── Desktop : buttons displayed normally ── */}
                        <div className="hidden sm:flex items-center gap-2">
                            <Button
                                variant="outline"
                                onClick={handleShare}
                                className="flex items-center gap-2"
                            >
                                <Share2 className="w-4 h-4" />
                                {getTranslation('common.share')}
                            </Button>

                            <Popover>
                                <PopoverTrigger asChild>
                                    <Button variant="outline" className="flex items-center gap-2">
                                        <Calendar className="w-4 h-4" />
                                        {getTranslation('common.export_calendar')}
                                    </Button>
                                </PopoverTrigger>
                                <PopoverContent align="end" sideOffset={8} className="w-64 p-3">
                                    <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-3">
                                        {getTranslation('common.export_calendar')}
                                    </p>
                                    <div className="flex flex-col gap-2">
                                        <button
                                            onClick={handleExportGoogle}
                                            className="flex items-center gap-3 w-full rounded-lg p-2.5 hover:bg-muted transition-colors text-left"
                                        >
                                            <div className="w-8 h-8 rounded-md bg-white border flex items-center justify-center shrink-0">
                                                <img src="https://www.google.com/favicon.ico" alt="Google" className="w-4 h-4" />
                                            </div>
                                            <div>
                                                <p className="text-sm font-medium">Google Calendar</p>
                                                <p className="text-xs text-muted-foreground">Opens in browser</p>
                                            </div>
                                        </button>

                                        <button
                                            onClick={handleExportOutlook}
                                            className="flex items-center gap-3 w-full rounded-lg p-2.5 hover:bg-muted transition-colors text-left"
                                        >
                                            <div className="w-8 h-8 rounded-md bg-[#0078D4] flex items-center justify-center shrink-0">
                                                <img src="https://outlook.com/favicon.ico" alt="Outlook" className="w-4 h-4" />
                                            </div>
                                            <div>
                                                <p className="text-sm font-medium">Outlook</p>
                                                <p className="text-xs text-muted-foreground">Opens in browser</p>
                                            </div>
                                        </button>

                                        <div className="border-t my-1" />

                                        <button
                                            onClick={handleExportIcs}
                                            className="flex items-center gap-3 w-full rounded-lg p-2.5 hover:bg-muted transition-colors text-left"
                                        >
                                            <div className="w-8 h-8 rounded-md bg-muted border flex items-center justify-center shrink-0">
                                                <Calendar className="w-4 h-4 text-muted-foreground" />
                                            </div>
                                            <div>
                                                <p className="text-sm font-medium">{getTranslation('common.export_ics')}</p>
                                                <p className="text-xs text-muted-foreground">Apple Calendar, Thunderbird...</p>
                                            </div>
                                        </button>
                                    </div>
                                </PopoverContent>
                            </Popover>

                            {currentUserId === activity.createdBy && (
                                <>
                                    <Button
                                        variant="outline"
                                        onClick={() => onEdit?.(activity)}
                                        className="flex items-center gap-2"
                                    >
                                        <Edit className="w-4 h-4" />
                                        {getTranslation('common.edit')}
                                    </Button>

                                    <Button
                                        variant="destructive"
                                        onClick={() => {
                                            if (confirm(getTranslation('activity.confirm_delete'))) {
                                                onDelete?.(activity.id);
                                            }
                                        }}
                                        className="flex items-center gap-2"
                                    >
                                        <Trash2 className="w-4 h-4" />
                                        {getTranslation('common.delete')}
                                    </Button>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </header>
    );
}
