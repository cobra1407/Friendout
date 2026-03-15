import type { Localisation } from "@/features/localisation/types/localisation.type";
import type { Image } from "@/features/activity/types/image.type";
import type { Comment } from "@/features/comment/types/comment.type";
import type { SubActivity } from "@/features/subActivity/types/subActivity.type";
import type { Equipment } from "@/features/equipment/types/equipment.type";
import type { UserParticipation } from "@/features/user/types/UserParticipation.type";
import type { Participant } from "@/features/participant/types/Participant.type";
import type { UserEquipment } from "@/features/equipment/types/userEquipment";

export interface ActivityDetails {
    activityEquipments: boolean;
    // === Activity core ===
    id: string;
    title: string;
    description: string;
    startAt: string;
    endAt?: string | null;
    estimatedPrice?: number | null;
    totalPrice?: number | null;

    image?: Image | null;
    localisation?: Localisation | null;

    // === Meta ===
    createdBy: string;
    createdAt: string;
    updatedAt: string;

    // === User participation ===
    userMainParticipation?: UserParticipation | null;

    userSubActivitiesParticipations?: UserParticipation[];

    // === Participants (main activity) ===
    participants: Participant[];

    // === Equipments ===
    requiredEquipments: Equipment[];

    // === User Equipments ===
    userEquipments: UserEquipment[];

    // === Sub activities ===
    subActivities: SubActivity[];

    // === Comments ===
    comments: Comment[];
}
