import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Users } from "lucide-react";
import type { Participant } from "../types/Participant.type";
import { ParticipationStatus } from "../enum/participationStatus.enum";
import { useEffect, useState } from "react";
import { Avatar, AvatarImage } from "@radix-ui/react-avatar";
import { getTranslation } from "@/i18n";


interface ParticipantsCardProps {
    participants: Participant[] | undefined;
    className?: string;
}

export default function ParticipantsCard({ participants, className }: ParticipantsCardProps) {
    const [participating, setParticipating] = useState<Participant[]>([]);
    const [maybe, setMaybe] = useState<Participant[]>([]);
    const [notParticipating, setNotParticipating] = useState<Participant[]>([]);
    const getParticipantsByStatus = (participationStatus: ParticipationStatus) => {
        if (!participants) return [];
        return participants.filter((participant) => participant.participationStatus === participationStatus);
    };

    useEffect(() => {
        setParticipating(getParticipantsByStatus(ParticipationStatus.Participating));
        setMaybe(getParticipantsByStatus(ParticipationStatus.Maybe));
        setNotParticipating(getParticipantsByStatus(ParticipationStatus.NotParticipating));
    }, [participants]);

    return (
        <Card className={className}>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <Users className="w-5 h-5" />
                    {getTranslation('participants.main_activity_title')}
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
                {participants && participants.length === 0 ? (
                    <div className="text-sm text-gray-500 text-center py-2">
                        {getTranslation('participants.none_yet')}
                    </div>
                ) : (
                    <div className="space-y-4">
                        {participating.length > 0 && (
                            <div>
                                <h4 className="font-medium text-green-600 mb-2">
                                    {getTranslation('participants.participating')} ({participating.length})
                                </h4>
                                <div className="space-y-1">
                                    {participating.map((participant) => (
                                        <div key={participant.participationId} className="text-sm flex gap-2 items-center">
                                            <Avatar className="w-7 h-7 rounded-full bg-red-50 flex items-center justify-center">
                                                {participant.avatarUrl ? (
                                                    <AvatarImage src={participant.avatarUrl} />
                                                ) : (
                                                    <span className="text-red-700 font-medium">
                                                        {participant.username ? participant.username[0].toUpperCase() + participant.username[1].toLocaleLowerCase() : "?"}
                                                    </span>
                                                )}
                                            </Avatar>
                                            <span>{participant.username}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {maybe.length > 0 && (
                            <div>
                                <h4 className="font-medium text-yellow-600 mb-2">
                                    {getTranslation('participants.maybe')} ({maybe.length})
                                </h4>
                                <div className="space-y-1">
                                    {maybe.map((participant) => (
                                        <div key={participant.participationId} className="text-sm flex gap-2 items-center">
                                            <Avatar className="w-7 h-7 rounded-full bg-red-50 flex items-center justify-center">
                                                {participant.avatarUrl ? (
                                                    <AvatarImage src={participant.avatarUrl} />
                                                ) : (
                                                    <span className="text-red-700 font-medium">
                                                        {participant.username ? participant.username.charAt(0).toUpperCase() : "?"}
                                                    </span>
                                                )}
                                            </Avatar>
                                            <span>{participant.username}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {notParticipating.length > 0 && (
                            <div>
                                <h4 className="font-medium text-red-600 mb-2">
                                    {getTranslation('participants.not_participating')} ({notParticipating.length})
                                </h4>
                                <div className="space-y-1">
                                    {notParticipating.map((participant) => (
                                        <div key={participant.participationId} className="text-sm flex gap-2 items-center">
                                            <Avatar className="w-7 h-7 rounded-full bg-red-50 flex items-center justify-center">
                                                {participant.avatarUrl ? (
                                                    <AvatarImage src={participant.avatarUrl} />
                                                ) : (
                                                    <span className="text-red-700 font-medium">
                                                        {participant.username ? participant.username.charAt(0).toUpperCase() : "?"}
                                                    </span>
                                                )}
                                            </Avatar>
                                            <span>{participant.username}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
