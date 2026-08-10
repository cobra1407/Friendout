import type { Localisation } from "@/features/localisation/types/localisation.type";
import type { Image } from "@/features/activity/types/image.type";

export interface PublicSubActivity {
    name: string;
    startTime: string;
    endTime: string;
    description?: string | null;
    price?: number | null;
    localisation?: Localisation | null;
}

export interface PublicParticipantsCount {
    participating: number;
    maybe: number;
    notParticipating: number;
}

export interface PublicActivity {
    activityId: string;
    title: string;
    description: string;
    startAt: string;
    endAt?: string | null;
    estimatedPrice?: number | null;
    image?: Image | null;
    localisation?: Localisation | null;
    createdBy: string;
    participantsCount: PublicParticipantsCount;
    subActivities: PublicSubActivity[];
    requiredEquipmentNames: string[];
}
