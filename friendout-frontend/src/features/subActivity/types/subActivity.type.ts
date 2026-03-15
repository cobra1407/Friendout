import type { Participant } from "@/features/participant/types/Participant.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";

export interface SubActivity {
    id: string;
    name: string;
    localisation?: Localisation | null;
    startTime: string;
    endTime: string;
    description: string;
    price: number;
    activityId: string;
    participants: Participant[];
}
