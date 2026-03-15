import type { Activity } from "@/features/activity/types/acitivity.type";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import type { Participant } from "@/features/participant/types/Participant.type";
import type { SubActivity } from "@/features/subActivity/types/subActivity.type";
import api from "@/lib/api/api";
import type { TimeFilter } from "@/features/activity/types/activityFilter.type";
import { pickLocalisation } from "@/features/activity/utils/localisation.utils";

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

export interface CreateActivityPayload {
  title: string;
  description: string;
  startAt: string;
  time: string;
  endAt?: string;
  estimatedPrice?: number;
  localisation: Localisation | null;
  activityImage?: File;
  requiredEquipmentNames?: string[];
  subActivities?: SubActivity[];
}

export interface UpdateActivityPayload extends CreateActivityPayload {}

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
  formData.append("Title", payload.title.trim());
  formData.append("Description", payload.description.trim());
  formData.append("StartAt", payload.startAt);
  formData.append("Time", payload.time);

  if (payload.endAt) {
    formData.append("EndAt", payload.endAt);
  }

  if (payload.estimatedPrice !== undefined && Number.isFinite(payload.estimatedPrice)) {
    formData.append("EstimatedPrice", String(payload.estimatedPrice));
  }

  if (payload.requiredEquipmentNames && payload.requiredEquipmentNames.length > 0) {
    const cleanedEquipmentNames = payload.requiredEquipmentNames
      .filter((name) => Boolean(name?.trim()))
      .map((name) => name.trim());

    cleanedEquipmentNames.forEach((name, index) => {
      formData.append("RequiredEquipmentNames", name);
      formData.append(`RequiredEquipmentNames[${index}]`, name);
    });
    formData.append("RequiredEquipmentNamesJson", JSON.stringify(cleanedEquipmentNames));
  }

  if (payload.subActivities && payload.subActivities.length > 0) {
    const normalizedSubActivities = payload.subActivities
      .filter((subActivity) => Boolean(subActivity.name?.trim()) && Boolean(subActivity.startTime) && Boolean(subActivity.endTime))
      .map((subActivity) => ({
        id: subActivity.id || null,
        name: subActivity.name.trim(),
        startTime: subActivity.startTime,
        endTime: subActivity.endTime,
        description: subActivity.description?.trim() || null,
        price: Number.isFinite(subActivity.price) ? subActivity.price : 0,
        address: subActivity.localisation?.type === LocalisationType.Address ? subActivity.localisation.address?.trim() || null : null,
        mapLink: subActivity.localisation?.type === LocalisationType.MapLink ? subActivity.localisation.mapLink?.trim() || null : null,
        virtualUrl: subActivity.localisation?.type === LocalisationType.Virtual
          ? (subActivity.localisation.virtualUrl?.trim() || subActivity.localisation.serverInfo?.trim() || null)
          : null
      }));

    if (normalizedSubActivities.length > 0) {
      formData.append("SubActivitiesJson", JSON.stringify(normalizedSubActivities));
    }
  }

  appendLocalisationFields(formData, payload.localisation);

  if (payload.activityImage) {
    formData.append("ActivityImage", payload.activityImage);
  }

  return formData;
};

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
