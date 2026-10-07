import { Button } from "@/components/ui/button";
import { Check, Clock3, X, Lock } from "lucide-react";
import { ParticipationStatus } from "../enum/participationStatus.enum";
import clsx from "clsx";
import { getTranslation } from "@/i18n";

type Props = {
    onResponse: (status: ParticipationStatus) => void;
    selectedStatus: ParticipationStatus | null | undefined;
    fullWidth?: boolean;
    canParticipate?: boolean;
    /** The activity has reached its participant limit: "I'm attending" is no longer offered to newcomers. */
    isFull?: boolean;
};

export function ParticipationButtons({
    onResponse,
    selectedStatus,
    fullWidth = true,
    canParticipate = true,
    isFull = false,
}: Props) {
    // Someone already attending keeps their spot: only others are blocked.
    const isParticipateDisabled =
        isFull && selectedStatus !== ParticipationStatus.Participating;

    return (
        <>
            {canParticipate ? (
                <div
                    className={clsx(
                        // Mobile first
                        "flex flex-col gap-2",
                        // Desktop
                        "md:flex-row md:items-center"
                    )}
                >
                    <Button
                        onClick={() => onResponse(ParticipationStatus.Participating)}
                        disabled={isParticipateDisabled}
                        title={isParticipateDisabled ? getTranslation('participation.full_hint') : undefined}
                        variant={
                            selectedStatus === ParticipationStatus.Participating
                                ? "default"
                                : "outline"
                        }
                        className={clsx(
                            "h-10 gap-2 border",
                            fullWidth && "w-full md:w-auto"
                        )}
                    >
                        <Check className="h-4 w-4" />
                        {isParticipateDisabled
                            ? getTranslation('participation.full')
                            : getTranslation('participation.i_participate')}
                    </Button>


                    <Button
                        onClick={() => onResponse(ParticipationStatus.Maybe)}
                        variant={
                            selectedStatus === ParticipationStatus.Maybe ? "default" : "outline"
                        }
                        className={clsx("h-10 gap-2 border", fullWidth && "w-full md:w-auto")}
                    >
                        <Clock3 className="h-4 w-4" />
                        {getTranslation('participation.maybe')}
                    </Button>

                    <Button
                        onClick={() => onResponse(ParticipationStatus.NotParticipating)}
                        variant={
                            selectedStatus === ParticipationStatus.NotParticipating
                                ? "destructive"
                                : "outline"
                        }
                        className={clsx("h-10 gap-2 border", fullWidth && "w-full md:w-auto")}>
                        <X className="h-4 w-4" />
                        {getTranslation('participation.no')}
                    </Button>
                </div>
            ) :
                <div className="mx-auto mb-2 inline-flex items-center gap-2 rounded-full bg-muted px-4 py-1 text-sm font-medium text-muted-foreground border-dotted border-2 border-border">
                    <Lock className="h-4 w-4" />
                    {getTranslation('participation.closed')}
                </div>
            }

        </>
    );
}
