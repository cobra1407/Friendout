import type { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";

export interface UserParticipation{
    activityId: string;
    subActivityId?: string;
    status: ParticipationStatus;
}
