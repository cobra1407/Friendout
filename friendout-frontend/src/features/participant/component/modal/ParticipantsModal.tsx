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
                return "bg-emerald-100 text-emerald-800 border-emerald-200";
            case ParticipationStatus.Maybe:
                return "bg-amber-100 text-amber-800 border-amber-200";
            case ParticipationStatus.NotParticipating:
                return "bg-rose-100 text-rose-800 border-rose-200";
            default:
                return "bg-gray-100 text-gray-800 border-gray-200";
        }
    };

    const getStatusLabel = (status: ParticipationStatus) => {
        switch (status) {
            case ParticipationStatus.Participating:
                return "Participe";
            case ParticipationStatus.Maybe:
                return "Peut-être";
            case ParticipationStatus.NotParticipating:
                return "Ne participe pas";
            default:
                return "Inconnu";
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
                    Participant{participants.length > 1 ? "s" : ""}
                </ModalTitle>
                <ModalDescription>
                    Liste des participants pour la sous activité «&nbsp;
                    {activityName}&nbsp;».
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
