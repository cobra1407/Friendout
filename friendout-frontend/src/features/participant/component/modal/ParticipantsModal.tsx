import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal";
import { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";
import type { Participant } from "@/features/participant/types/Participant.type";
import { getTranslation } from "@/i18n";

interface ParticipantsModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    participants: Participant[];
    activityName: string;
}

export function ParticipantsModal({
    open,
    onOpenChange,
    participants,
    activityName,
}: ParticipantsModalProps) {
    const participantsByStatus = {
        [ParticipationStatus.Participating]: participants.filter(
            (p) => p.participationStatus === ParticipationStatus.Participating
        ),
        [ParticipationStatus.Maybe]: participants.filter(
            (p) => p.participationStatus === ParticipationStatus.Maybe
        ),
        [ParticipationStatus.NotParticipating]: participants.filter(
            (p) => p.participationStatus === ParticipationStatus.NotParticipating
        ),
    };

    const getStatusColor = (status: ParticipationStatus) => {
        switch (status) {
            case ParticipationStatus.Participating:
                return "bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/30";
            case ParticipationStatus.Maybe:
                return "bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-500/30";
            case ParticipationStatus.NotParticipating:
                return "bg-rose-500/15 text-rose-700 dark:text-rose-400 border-rose-500/30";
            default:
                return "bg-muted text-muted-foreground border-border";
        }
    };

    const getStatusLabel = (status: ParticipationStatus) => {
        switch (status) {
            case ParticipationStatus.Participating:
                return getTranslation('participants.participating');
            case ParticipationStatus.Maybe:
                return getTranslation('participants.maybe');
            case ParticipationStatus.NotParticipating:
                return getTranslation('participants.not_participating');
            default:
                return getTranslation('participants.unknown');
        }
    };

    return (
        <Modal
            open={open}
            onClose={() => onOpenChange(false)}
            className="sm:max-w-[480px] max-h-[85vh] overflow-y-auto"
        >
            <ModalHeader>
                <ModalTitle className="text-xl">
                    {getTranslation('participants.modal_title')}
                </ModalTitle>
                <ModalDescription>
                    {getTranslation('participants.modal_description', { name: activityName })}
                </ModalDescription>
            </ModalHeader>

            <div className="space-y-6 py-4">
                {Object.entries(participantsByStatus).map(([statusKey, group]) => {
                    const status = statusKey as ParticipationStatus;
                    if (group.length === 0) return null;

                    return (
                        <div key={status} className="space-y-3">
                            <div className="flex items-center gap-2">
                                <div className={`px-3 py-1 rounded-full text-xs font-medium border ${getStatusColor(status)}`}>
                                    {getStatusLabel(status)} ({group.length})
                                </div>
                            </div>

                            <div className="grid grid-cols-1 gap-1.5">
                                {group.map((p) => (
                                    <div
                                        key={p.participationId}
                                        className="flex items-center gap-3 p-2.5 rounded-lg hover:bg-accent/60 transition-colors"
                                    >
                                        <Avatar className="h-10 w-10 border-2 border-background">
                                            <AvatarImage src={p.avatarUrl} alt={p.username} />
                                            <AvatarFallback className="text-base">
                                                {p.username[0].toUpperCase() + p.username[1]}
                                            </AvatarFallback>
                                        </Avatar>
                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-medium leading-tight truncate">
                                                {p.username}
                                            </p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    );
                })}

                {participants.length === 0 && (
                    <div className="text-center py-8 text-muted-foreground">
                        {getTranslation('participants.no_participants')}
                    </div>
                )}
            </div>
        </Modal>
    );
}
