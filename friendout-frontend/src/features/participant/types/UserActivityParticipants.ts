import type { Participant } from "./Participant.type";

export interface UserActivityParticipants
{
    mainActivityParticipants: Participant[],
    subActivityParticipants: Participant[]
}
