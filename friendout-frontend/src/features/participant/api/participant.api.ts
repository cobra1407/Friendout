import type { UpsertParticipationPayload } from "@/features/participant/types/UpsertParticipationPayload";
import api  from "@/lib/api/api";
import type { UserActivityParticipation } from "../types/UserParticipation.type";
import type { UserActivityParticipants } from "../types/UserActivityParticipants";

export async function getActivityParticipants(activityId: string) : Promise<UserActivityParticipants> {
    const response = await api.get<UserActivityParticipants>(`/activities/${activityId}/participants`);
    return response.data;
}


export async function UpsertParticipation(payload: UpsertParticipationPayload) : Promise<UserActivityParticipation>
{
    const response = await api.put<UserActivityParticipation>(`/activities/${payload.activityId}/participation`, payload);
    return response.data;
}
