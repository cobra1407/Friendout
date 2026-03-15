import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Calendar, Clock, Users, MapPin, MessageCircle, ExternalLink, ArrowRight } from "lucide-react";
import type { SubActivityDetails } from "@/features/subActivity/types/subActivityDetails.type";
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import { formatTime, formatDate } from "@/lib/utils/date.utils";
import { ParticipationButtons } from "@/features/participant/component/ParticipationButtons";
import { ParticipationStatus } from "@/features/participant/enum/participationStatus.enum";
import { useState } from "react";
import { ParticipantsModal } from "@/features/participant/component/modal/ParticipantsModal";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { getTranslation } from "@/i18n";
import { getLocalisationDisplayText } from "@/lib/maps";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

interface SubActivityDetailsCardProps {
    subActivity: SubActivityDetails;
    maxVisibleParticipants?: number;
    onResponse: (participationStatus: ParticipationStatus, subActivityIds: string[]) => void;
    selectedStatus: ParticipationStatus | null;
}


const SubActivityDetailsCard = ({ subActivity, maxVisibleParticipants, onResponse, selectedStatus }: SubActivityDetailsCardProps) => {
    const [showParticipantModal, setShowParticipantModal] = useState(false);
    const max = maxVisibleParticipants ?? 5;
    const canParticipate = new Date(subActivity.startTime) > new Date();

    const participatingParticipants = subActivity.participants.filter(
        (p) =>
            p.participationStatus === ParticipationStatus.Participating ||
            p.participationStatus === ParticipationStatus.Maybe
    );

    const visibleParticipants = participatingParticipants.slice(0, max);

    const showMoreCount =
        participatingParticipants.length > visibleParticipants.length
            ? participatingParticipants.length - visibleParticipants.length
            : 0;

    const handleResponse = (status: ParticipationStatus) => {
        onResponse(status, [subActivity.id]);
    }

    const localisationData = subActivity.localisation ?? null;
    const localisationDisplayText = getLocalisationDisplayText(localisationData);
    const isGoogleMapsLink = localisationData?.type === LocalisationType.MapLink || !!localisationData?.mapLink;
    const startAt = new Date(subActivity.startTime);
    const endAt = subActivity.endTime ? new Date(subActivity.endTime) : null;
    const canShowEndInfo =
        endAt !== null &&
        !Number.isNaN(startAt.getTime()) &&
        !Number.isNaN(endAt.getTime()) &&
        endAt > startAt;

    const toHourLabel = (date: Date) => formatTime(date).replace(":", "h");

    const formatDuration = () => {
        if (!canShowEndInfo || !endAt) return null;
        const diffInMinutes = Math.floor((endAt.getTime() - startAt.getTime()) / (1000 * 60));
        const hours = Math.floor(diffInMinutes / 60);
        const minutes = diffInMinutes % 60;
        return `${hours}h${minutes.toString().padStart(2, "0")}`;
    };

    const duration = formatDuration();
    const startLabel = toHourLabel(startAt);
    const endLabel = canShowEndInfo && endAt ? toHourLabel(endAt) : null;

    const openInMaps = () => {
        if (!localisationData?.mapLink) return;
        window.open(localisationData.mapLink, "_blank");
    };

    return (
        <Card className="w-full border shadow-md hover:shadow-lg transition-shadow duration-300 bg-card">
            <CardHeader className="pb-4">
                <div className="flex justify-between items-start gap-4">
                    <div className="space-y-1.5">
                        <CardTitle className="text-2xl font-bold leading-none tracking-tight">
                            {subActivity.name}
                        </CardTitle>

                        <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-muted-foreground text-sm">
                            <div className="flex items-center gap-1.5">
                                <Calendar className="h-4 w-4 text-primary/70" />
                                <span>{formatDate(subActivity.startTime)}</span>
                            </div>
                            <div className="flex items-center gap-1.5">
                                <Clock className="h-4 w-4 text-primary/70" />
                                <span>{startLabel}</span>
                                {endLabel && (
                                    <>
                                        <ArrowRight className="h-3.5 w-3.5 text-primary/70" />
                                        <span>{endLabel}</span>
                                    </>
                                )}
                                {duration && (
                                    <>
                                        <ArrowRight className="h-3.5 w-3.5 text-primary/70" />
                                        <Badge variant="outline" className="text-xs">
                                            {duration}
                                        </Badge>
                                    </>
                                )}
                            </div>
                        </div>
                    </div>
                    <div className="shrink-0">
                        <div className={`px-2.5 py-1 rounded-full border text-[10px] sm:text-xs font-bold uppercase tracking-widest ${subActivity.price > 0
                            ? "border-orange-200 bg-orange-50 text-orange-700"
                            : "border-emerald-200 bg-emerald-50 text-emerald-700"
                            }`}>
                            {subActivity.price > 0 ? `${subActivity.price}€` : getTranslation('common.free')}
                        </div>
                    </div>
                </div>
            </CardHeader>

            <CardContent className="space-y-8">
                <div className="relative">
                    <div className="absolute -left-6 top-0 bottom-0 w-1 bg-primary/10 rounded-r-full -my-2" />
                    {localisationData && (
                        <div className="flex items-center gap-1.5 font-medium text-foreground/80">
                            {localisationData.type === LocalisationType.Virtual ? (
                                <MessageCircle className="h-4 w-4 text-blue-600" />
                            ) : (
                                <MapPin className="h-4 w-4 text-red-700 -translate-x-1" />
                            )}
                            <span className="line-clamp-1 flex-1" title={localisationDisplayText}>
                                {localisationDisplayText}
                            </span>
                            {localisationData.type === LocalisationType.Virtual && (
                                <Badge variant="outline" className="text-xs">
                                    {getTranslation('activity.virtual_place')}
                                </Badge>
                            )}
                            {isGoogleMapsLink && (
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={openInMaps}
                                    className="p-1 h-6 w-6 shrink-0 cursor-pointer"
                                    title={getTranslation('activity.open_google_maps')}
                                >
                                    <ExternalLink className="w-3 h-3" />
                                </Button>
                            )}
                        </div>
                    )}
                    <h4 className="text-xs font-bold uppercase tracking-widest mb-2">
                        {getTranslation('sub_activity.details')}
                    </h4>
                    <p className="text-sm leading-relaxed text-foreground/80 max-w-2xl">
                        {subActivity.description || getTranslation('sub_activity.no_description')}
                    </p>
                </div>
                <div className="flex flex-wrap items-end justify-between gap-6 pt-2">

                    {/* Participants section (Handle the case "0") */}
                    <div className="space-y-3">
                        <h4 className="text-xs font-bold uppercase tracking-widest text-muted-foreground gap-2">
                            {participatingParticipants.length > 0
                                ? `${getTranslation('sub_activity.participants')} (${participatingParticipants.length})`
                                : ""}
                        </h4>

                        {participatingParticipants.length > 0 ? (
                            <div
                                className="flex items-center gap-3 group cursor-pointer"
                                onClick={() => setShowParticipantModal(true)}
                            >
                                <div className="flex -space-x-2">
                                    {visibleParticipants.map((p) => (
                                        <Tooltip key={p.participationId}>
                                            <TooltipTrigger asChild>
                                                <Avatar
                                                    className="h-8 w-8 ring-2 ring-background transition group-hover:opacity-80 hover:translate-y-[-5px]"
                                                >
                                                    <AvatarImage src={p.avatarUrl} />
                                                    <AvatarFallback className=" border text-slate-800 font-medium text-red-700">
                                                        {p.username[0].toUpperCase() + p.username[1]}
                                                    </AvatarFallback>
                                                </Avatar>
                                            </TooltipTrigger>
                                            <TooltipContent>
                                                <p>{p.username}</p>
                                            </TooltipContent>
                                        </Tooltip>
                                    ))}

                                </div>
                                {showMoreCount > 0 && (
                                    <button
                                        type="button"
                                        className="text-xs font-medium text-muted-foreground hover:text-foreground hover:bg-primary/5 px-2 py-1 rounded-lg transition hover:cursor-pointer"
                                    >
                                        +{showMoreCount} {getTranslation('sub_activity.more_others')}
                                    </button>
                                )}
                            </div>
                        ) : (
                            <div className="flex items-center gap-2 text-sm text-muted-foreground bg-muted/50 px-3 py-1.5 rounded-lg border border-dashed">
                                <Users className="h-4 w-4" />
                                <span onClick={() => setShowParticipantModal(true)} className="cursor-pointer">
                                    {getTranslation('sub_activity.no_participant_be_first')}
                                </span>
                            </div>
                        )}
                    </div>

                    <div className="w-full md:w-auto flex flex-col md:flex-row md:justify-end">
                        <ParticipationButtons
                            onResponse={handleResponse}
                            selectedStatus={selectedStatus}
                            fullWidth={false}
                            canParticipate={canParticipate}
                        />
                    </div>
                    <ParticipantsModal
                        open={showParticipantModal}
                        onOpenChange={setShowParticipantModal}
                        participants={subActivity.participants}
                        activityName={subActivity.name}
                    />
                </div>
            </CardContent>
        </Card >
    );
};

export default SubActivityDetailsCard;
