import type { Localisation } from "@/features/localisation/types/localisation.type";
import type { SubActivity } from "@/features/subActivity/types/subActivity.type";
import type { Image } from "@/features/activity/types/image.type";
import type { Comment } from "@/features/comment/types/comment.type";

export interface Activity {
    id: string;
    title: string;
    description: string;
    startAt: string;
    endAt: string;
    subActivities?: SubActivity[];
    localisation: Localisation;
    estimatedPrice: number;
    image?: Image;
    createdBy: string;
    createdAt: string;
    updatedAt: string;
    hasEquipment: boolean;
    nbParticipants: number;
    comments?: Array<Comment>
}
