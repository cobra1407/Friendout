import type { Activity } from "@/features/activity/types/activity.type";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Participant } from "@/features/participant/types/Participant.type";
import type { SubActivity } from "@/features/subActivity/types/subActivity.type";
import api from "@/lib/api/api";
import type { TimeFilter } from "@/features/activity/types/activityFilter.type";
import { pickLocalisation } from "@/features/localisation/utils/localisation.utils";
import type { CreateActivityFormData } from "../schema/createActivity.schema";

export type CreateActivityPayload = CreateActivityFormData;
export type UpdateActivityPayload = CreateActivityPayload;

// ====================== MAPPING & HELPERS ======================
interface GetActivitiesParams {
  skip?: number;
  take?: number;
  search?: string;
  timeFilter?: TimeFilter;
  onlyOwnActivity?: boolean;
}

type ApiSubActivity = Omit<SubActivity, "localisation"> & {
  location?: Localisation | null;
  localisation?: Localisation | null;
};

type ApiActivity = Omit<Activity, "localisation" | "subActivities"> & {
  location?: Localisation | null;
  localisation?: Localisation | null;
  subActivities?: ApiSubActivity[];
};

type ApiActivityDetails = Omit<ActivityDetails, "localisation" | "subActivities"> & {
  location?: Localisation | null;
  localisation?: Localisation | null;
  subActivities: ApiSubActivity[];
};

const dedupeParticipants = (participants?: Participant[] | null): Participant[] => {
  if (!participants?.length) return [];
  const byKey = new Map<string, Participant>();
  participants.forEach((participant) => {
    const key =
      participant.participationId ||
      `${participant.userId}:${participant.subActivityId ?? "main"}:${participant.participationStatus}`;
    byKey.set(key, participant);
  });
  return Array.from(byKey.values());
};

const mapSubActivityFromApi = (subActivity: ApiSubActivity): SubActivity => ({
  ...subActivity,
  localisation: pickLocalisation(subActivity),
  participants: dedupeParticipants(subActivity.participants),
});

const mapActivityFromApi = (activity: ApiActivity): Activity => ({
  ...activity,
  localisation: pickLocalisation(activity) ?? { type: LocalisationType.Address },
  subActivities: activity.subActivities?.map(mapSubActivityFromApi),
  comments: activity.comments ?? [],
});

const mapActivityDetailsFromApi = (activity: ApiActivityDetails): ActivityDetails => ({
  ...activity,
  localisation: pickLocalisation(activity),
  participants: dedupeParticipants(activity.participants),
  subActivities: activity.subActivities?.map(mapSubActivityFromApi) ?? [],
  comments: activity.comments ?? [],
});

// ====================== FORM DATA BUILDER ======================
const appendLocalisationFields = (formData: FormData, localisation: Localisation | null) => {
  if (!localisation) return;

  if (localisation.type === LocalisationType.Address) {
    if (localisation.address?.trim()) formData.append("Address", localisation.address.trim());
    return;
  }
  if (localisation.type === LocalisationType.MapLink) {
    if (localisation.mapLink?.trim()) formData.append("MapLink", localisation.mapLink.trim());
    return;
  }
  if (localisation.type === LocalisationType.Virtual) {
    const virtualUrl = localisation.virtualUrl?.trim() || localisation.serverInfo?.trim();
    if (virtualUrl) formData.append("VirtualUrl", virtualUrl);
  }
};

const buildActivityFormData = (payload: CreateActivityPayload): FormData => {
  const formData = new FormData();

  formData.append("Title", payload.title);
  formData.append("Description", payload.description);
  formData.append("StartAt", payload.startAt.toISOString());
  formData.append("Time", payload.time);

  if (payload.endAt) formData.append("EndAt", payload.endAt.toISOString());
  if (payload.estimatedPrice !== undefined) {
    formData.append("EstimatedPrice", String(payload.estimatedPrice));
  }

  // Required Equipment Names
  if (payload.requiredEquipmentNames.length > 0) {
    payload.requiredEquipmentNames.forEach((name, index) => {
      formData.append(`RequiredEquipmentNames[${index}]`, name);
    });
    // Fallback JSON (à supprimer plus tard si le binding indexé suffit)
    formData.append("RequiredEquipmentNamesJson", JSON.stringify(payload.requiredEquipmentNames));
  }

  // SubActivities
  if (payload.subActivities.length > 0) {
    const normalizedSubActivities = payload.subActivities.map((subActivity) => ({
      id: subActivity.id || null,
      name: subActivity.name,
      startTime: subActivity.startTime,
      endTime: subActivity.endTime,
      description: subActivity.description || null,
      price: subActivity.price,
      address: subActivity.localisation?.type === LocalisationType.Address
        ? subActivity.localisation.address?.trim() || null
        : null,
      mapLink: subActivity.localisation?.type === LocalisationType.MapLink
        ? subActivity.localisation.mapLink?.trim() || null
        : null,
      virtualUrl: subActivity.localisation?.type === LocalisationType.Virtual
        ? (subActivity.localisation.virtualUrl?.trim() || subActivity.localisation.serverInfo?.trim() || null)
        : null,
    }));

    formData.append("SubActivitiesJson", JSON.stringify(normalizedSubActivities));
  }

  appendLocalisationFields(formData, payload.localisation);

  if (payload.activityImage) {
    formData.append("ActivityImage", payload.activityImage);
  }

  return formData;
};

// ====================== API FUNCTIONS ======================
export async function getActivities(params?: GetActivitiesParams): Promise<Activity[]> {
  const query = new URLSearchParams();
  if (params?.skip !== undefined) query.append("skip", params.skip.toString());
  if (params?.take !== undefined) query.append("take", params.take.toString());
  if (params?.search) query.append("search", params.search);
  if (params?.timeFilter) query.append("timeFilter", params.timeFilter);
  if (params?.onlyOwnActivity) query.append("onlyOwnActivity", "true");

  const response = await api.get<ApiActivity[]>(`/activities?${query.toString()}`);
  return response.data.map(mapActivityFromApi);
}

export async function getActivityById(id: string): Promise<ActivityDetails> {
  const response = await api.get<ApiActivityDetails>(`/activities/${id}/details`);
  return mapActivityDetailsFromApi(response.data);
}

// La validation Zod est faite dans le formulaire (ActivityForm) avant l'appel API.
// Ces fonctions reçoivent un payload déjà validé et construisent le FormData.
export async function createActivity(payload: CreateActivityPayload): Promise<Activity> {
  const formData = buildActivityFormData(payload);
  const response = await api.post<ApiActivity>("/activities", formData);
  return mapActivityFromApi(response.data);
}

export async function updateActivity(activityId: string, payload: UpdateActivityPayload): Promise<Activity> {
  const formData = buildActivityFormData(payload);
  const response = await api.put<ApiActivity>(`/activities/${activityId}`, formData);
  return mapActivityFromApi(response.data);
}
