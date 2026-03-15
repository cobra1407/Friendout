import type { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";

export interface UpsertParticipationPayload {
  activityId: string;
  status: ParticipationStatus;
  subActivityIds?: string[] | null;
}
