import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";
import { ParticipationButtons } from "@/features/participant/component/ParticipationButtons";

type Props = {
    onResponse: (participationStatus: ParticipationStatus) => void;
    selectedStatus?: ParticipationStatus | null;
    title: string,
    description?: string,
    isFull?: boolean
};

export default function ParticipationResponseCard({ onResponse, selectedStatus, title, description, isFull }: Props) {
    return (
        <Card>
            <CardHeader>
                <CardTitle>{title}</CardTitle>
                {description && <p className="text-sm text-muted-foreground">{description}</p>}
            </CardHeader>
            <CardContent className="space-y-3">
                <ParticipationButtons onResponse={onResponse} selectedStatus={selectedStatus} isFull={isFull} />
            </CardContent>
        </Card>
    );
}
