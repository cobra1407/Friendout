import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Calendar, Clock, MapPin, Euro, Users, ExternalLink, MessageCircle } from 'lucide-react';
import { type Activity } from '@/features/activity/types/activity.type';
import { LocalisationType } from '@/features/localisation/types/localisation.type';
import { getLocalisationDisplayText } from '@/lib/maps';
import { useEffect, useState } from 'react';
import { formatDate, formatTime } from '@/lib/utils/date.utils';
import defaultActivityImage from '@/assets/images/default-activity-card.png';
import { resolveMediaUrl } from '@/lib/media';
import { getTranslation } from '@/i18n';

interface ActivityCardProps {
    activity: Activity;
    onViewDetails: (id: string) => void;
}

export default function ActivityCard({ activity, onViewDetails }: ActivityCardProps) {
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const isPast = new Date(activity.startAt) < new Date();

    useEffect(() => {
        if (activity) {
            setIsLoading(false);
        }
    }, [activity]);

    const openInMaps = (e: React.MouseEvent) => {
        e.stopPropagation();
        if (!activity.localisation.mapLink) return;

        window.open(activity.localisation.mapLink, '_blank');
    };

    const localisationData = activity.localisation ?? null;
    const localisationDisplayText = getLocalisationDisplayText(activity.localisation);
    const isGoogleMapsLink = localisationData?.type === LocalisationType.MapLink || !!localisationData?.mapLink;
    const subActivitiesCount = activity.subActivities?.length || 0;

    return (
        <Card className={`flex flex-col max-h-[800px] max-w-[500px] hover:shadow-lg transition-shadow ${isPast ? 'opacity-75' : ''}`}>
            <CardHeader className="pb-3">
                <div className="flex flex-col gap-2">
                    {/* Badges */}
                    <div className="flex flex-wrap gap-2">
                        {isPast && <Badge variant="secondary">{getTranslation('activity.past')}</Badge>}
                        {activity.hasEquipment && (
                            <Badge variant="outline" className="text-xs bg-blue-200">
                                {getTranslation('activity.equipment_required')}
                            </Badge>
                        )}
                        {subActivitiesCount > 0 && (
                            <Badge variant="outline" className="text-xs">
                                {subActivitiesCount} {subActivitiesCount === 1 ? getTranslation('activity.sub_activity_one') : getTranslation('activity.sub_activities')}
                            </Badge>
                        )}
                    </div>

                    {/* Titre */}
                    <CardTitle className="text-lg font-semibold line-clamp-2">
                        {activity.title}
                    </CardTitle>
                </div>
            </CardHeader>

            <CardContent className="flex-grow space-y-3">
                <div
                    className="relative w-full h-32 rounded-md overflow-hidden cursor-pointer"
                    onClick={() => onViewDetails(activity.id)}
                >
                    <img
                        src={resolveMediaUrl(activity.image?.url) ?? defaultActivityImage}
                        alt={activity.image?.altText ?? activity.title}
                        className="absolute inset-0 w-full h-full object-cover h-40 "
                        loading="lazy"
                    />

                    {/* Overlay si activité passée */}
                    {isPast && (
                        <div className="absolute inset-0 bg-gray-100/30 pointer-events-none" />
                    )}
                </div>

                <p className="text-sm text-muted-foreground line-clamp-2 min-h-[2.5rem]">
                    {activity.description}
                </p>
                <div className="space-y-2 text-sm">
                    <div className="flex items-center gap-2">
                        <Calendar className="w-4 h-4 text-blue-600" />
                        <span>{formatDate(activity.startAt)}</span>
                    </div>

                    <div className="flex items-center gap-2">
                        <Clock className="w-4 h-4 text-green-600" />
                        <span>{formatTime(activity.startAt)}</span>
                    </div>

                    <div className="flex items-center gap-2">
                        {localisationData?.type === LocalisationType.Virtual ? (
                            <MessageCircle className="w-4 h-4 text-blue-600" />
                        ) : (
                            <MapPin className="w-4 h-4 text-red-700" />
                        )}
                        <span className="line-clamp-1 flex-1" title={localisationDisplayText}>
                            {localisationDisplayText}
                        </span>
                        {localisationData?.type === LocalisationType.Virtual && (
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
                                title={isGoogleMapsLink ? getTranslation('activity.open_google_maps') : getTranslation('activity.view_google_maps')}
                            >
                                <ExternalLink className="w-3 h-3" />
                            </Button>
                        )}
                    </div>

                    <div className="flex items-center gap-2">
                        {activity.estimatedPrice > 0 ? (
                            <>
                                <Euro className="w-4 h-4 text-yellow-600" />
                                <span className="font-medium text-yellow-600">
                                    {activity.estimatedPrice.toFixed(2)}€
                                </span>
                            </>
                        ) : (
                            <>
                                <Euro className="w-4 h-4 text-green-600" />
                                <span className="font-medium text-green-600">
                                    {getTranslation('common.free')}
                                </span>
                            </>
                        )}
                    </div>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <Users className="w-4 h-4 text-purple-600" />
                            <span>
                                {isLoading
                                    ? getTranslation('common.loading')
                                    : `${activity.nbParticipants ?? 0} ${(activity.nbParticipants ?? 0) !== 1 ? getTranslation('activity.participants') : getTranslation('activity.participant')}`}
                            </span>

                        </div>
                    </div>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <span className="text-xs text-muted-foreground mt-1">
                                <Users className="w-3 h-3" />
                            </span>
                            <span className="text-xs text-muted-foreground mt-1">
                                {getTranslation('activity.created_by')} <span className="font-medium">{activity.createdBy}</span>
                            </span>
                        </div>
                    </div>
                </div>
                {/* Sub-activities preview */}
                {activity.subActivities && activity.subActivities.length > 0 ? (
                    <div className="mt-3 pt-3 border-t">
                        <div className="text-xs text-muted-foreground mb-2">{getTranslation('activity.sub_activities_label')}</div>
                        <div className="space-y-1 overflow-auto">
                            {activity.subActivities.slice(0, 2).map((subActivity) => (
                                <div key={subActivity.id} className="text-xs bg-gray-50 rounded px-2 py-1">
                                    <div className="flex justify-between items-center">
                                        <span className="font-medium">{subActivity.name}</span>
                                        {subActivity.price ? (
                                            <span className="text-green-600 font-medium text-yellow-600 vertical-align">
                                                {Number(subActivity.price).toFixed(2)}€
                                            </span>
                                        ) : (
                                            <span className="text-green-600 font-medium"> {getTranslation('common.free')} </span>
                                        )}
                                    </div>
                                    <span className="text-muted-foreground text-xs">
                                        {formatTime(subActivity.startTime.toString()) || getTranslation('activity.not_specified')}
                                        {subActivity.endTime
                                            ? ` - ${formatTime(subActivity.endTime.toString()) || getTranslation('activity.not_specified')}`
                                            : ` - ${getTranslation('activity.end_time_not_specified')}`}
                                    </span>
                                </div>
                            ))}
                            {activity.subActivities.length > 2 && (
                                <div className="text-xs text-muted-foreground">
                                    +{activity.subActivities.length - 2} {getTranslation('activity.more_others')}...
                                </div>
                            )}
                        </div>
                    </div>
                ) : (
                    <div className="mt-3 pt-3 border-t flex-grow flex flex-col items-center justify-center min-h-[100px]">
                        <span className="text-xs text-muted-foreground">{getTranslation('activity.no_sub_activity')}</span>
                    </div>
                )}
            </CardContent>
            <CardFooter>
                <Button
                    onClick={() => onViewDetails(activity.id)}
                    className="w-full"
                    variant={isPast ? "outline" : "default"}
                >
                    {getTranslation('activity.view_details')}
                </Button>
            </CardFooter>
        </Card>
    );
}
