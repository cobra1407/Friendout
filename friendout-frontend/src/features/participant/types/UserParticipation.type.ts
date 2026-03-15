import type { UserParticipation } from '@/features/user/types/UserParticipation.type';
import type { Participant } from './Participant.type';

export interface UserActivityParticipation {
    // participation
    userMainParticipation: UserParticipation;
    userSubActivitiesParticipations: UserParticipation[] | null;

    // participants
    mainActivityParticipants: Participant[] | null;
    subActivitiesParticipants: Participant[] | null;
}
