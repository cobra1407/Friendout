import type { ParticipationStatus } from "../enum/participationStatus.enum";

export interface Participant{
    participationId: string;
    userId: string;
    username: string;
    avatarUrl: string;
    participationStatus: ParticipationStatus;
    subActivityId?: string;
}
