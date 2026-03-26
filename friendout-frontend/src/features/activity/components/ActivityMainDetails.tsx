import { Calendar, Clock, Euro, ExternalLink, Link, MapPin, MessageCircle, Package, Users } from "lucide-react";
import DefaultActivityImage from "@/assets/images/default-activity-card.png";
import Linkify from 'linkify-react';

import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import { LocalisationType } from "@/features/localisation/types/localisation.type";
import { isPast, formatDate, formatTime } from "@/lib/utils/date.utils";

import { Badge } from "@/components/ui/badge";
import { getTranslation } from "@/i18n";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { resolveMediaUrl } from "@/lib/media";

type ActivityMainDetailsProps = {
    activity: ActivityDetails;
    maxEquipmentVisible?: number;
};

export default function ActivityMainDetails({ activity, maxEquipmentVisible }: ActivityMainDetailsProps) {
    const isGoogleMapsLink = activity?.localisation?.type === LocalisationType.MapLink;
    const MAX_EQUIPMENT_VISIBLE = maxEquipmentVisible ?? 10;

    const openInMaps = () => {
        const mapsUrl = activity.localisation?.mapLink;
        window.open(mapsUrl, "_blank");
    };

    const linkifyOptions = {
        target: '_blank',
        rel: 'noopener noreferrer',
        className: 'text-blue-800 hover:underline',
    };

    return (
        <Card>
            <CardHeader>
                <div className="flex justify-between items-start">
                    <CardTitle className="text-2xl">{activity.title}</CardTitle>
                    {isPast(activity.startAt) && <Badge variant="secondary">{getTranslation('activity.past')}</Badge>}
                </div>
            </CardHeader>
            <CardContent className="space-y-6">
                <div className="w-full h-64 rounded-lg overflow-hidden relative">
                    <img src={resolveMediaUrl(activity.image?.url) ?? DefaultActivityImage} alt={activity.title} />
                </div>

                <div className="space-y-4">
                    <div className="flex items-center gap-2">
                        <Calendar className="w-5 h-5 text-blue-600" />
                        <span className="font-medium">{formatDate(activity.startAt)}</span>
                    </div>

                    <div className="flex items-center gap-2">
                        <Clock className="w-4 h-4 text-green-600" />
                        <span>{formatTime(activity.startAt)}</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <div className="flex items-center gap-1">
                            {activity.localisation?.type === LocalisationType.Virtual ? (
                                <>
                                    <span>
                                        <MessageCircle className="w-5 h-5 text-blue-600" />
                                    </span>
                                    <span>
                                        {activity.localisation && activity.localisation.displayName}
                                    </span>
                                </>

                            ) : (
                                <>
                                    <MapPin className="w-5 h-5 text-red-600" />
                                    {isGoogleMapsLink && <Link className="w-4 h-4 text-blue-600" />}
                                </>
                            )}
                        </div>
                        <span className="flex-1" title={activity.localisation?.address}>
                            {activity.localisation && activity.localisation.address}
                        </span>
                        {activity.localisation?.type === LocalisationType.Virtual ? (
                            <Badge variant="outline">{getTranslation('activity.virtual_place')}</Badge>
                        ) : (
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={openInMaps}
                                className="flex items-center gap-1"
                                title={isGoogleMapsLink ? getTranslation('activity.open_google_maps') : getTranslation('activity.view_google_maps')}
                            >
                                <ExternalLink className="w-4 h-4" />
                                {isGoogleMapsLink ? getTranslation('activity.open') : getTranslation('activity.maps')}
                            </Button>
                        )}
                    </div>

                    <div className="flex items-center gap-3">
                        {activity.totalPrice && activity.totalPrice > 0 ? (
                            <>
                                <Euro className="w-5 h-5 text-yellow-600" />
                                <div className="flex flex-col">
                                    <span className="font-medium text-lg text-yellow-600">{getTranslation('activity.total')}: {Number(activity.totalPrice).toFixed(2)}€</span>
                                    {activity.subActivities && activity.subActivities.some((sub) => sub.price) && (
                                        <div className="text-sm text-muted-foreground">
                                            <span>
                                                {getTranslation('activity.main_activity_label')} {(activity.estimatedPrice || 0) > 0
                                                    ? <span className="font-bold text-gray-600">{(activity.estimatedPrice || 0).toFixed(2)}€</span>
                                                    : getTranslation('common.free')}
                                            </span>
                                            {activity.subActivities.filter((sub) => sub.price).length > 0 && (
                                                <span className="ml-2">
                                                    + {activity.subActivities.filter((sub) => sub.price).length} {getTranslation('activity.sub_activities_count')}
                                                </span>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </>
                        ) : (
                            <>
                                <Euro className="w-5 h-5 text-green-600" />
                                <span className="font-medium text-lg text-green-600">{getTranslation('common.free')}</span>
                            </>
                        )}
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

                <Separator />

                <div>
                    <h3 className="font-semibold mb-3">{getTranslation('activity.description')}</h3>
                    <Linkify options={linkifyOptions}>
                        <p className="text-gray-700 whitespace-pre-wrap">{activity.description}</p>
                    </Linkify>
                </div>

                {Array.isArray(activity.activityEquipments) && activity.activityEquipments.length > 0 && (
                    <>
                        <Separator />
                        <div>
                            <h3 className="font-semibold mb-3 flex items-center gap-2">
                                <Package className="w-5 h-5" />
                                {getTranslation('activity.necessary_equipment')}
                            </h3>
                            <div className="flex flex-wrap gap-2">
                                {activity.activityEquipments.slice(0, MAX_EQUIPMENT_VISIBLE).map((equipment, index) => (
                                    <Badge key={equipment.equipmentId || index} variant="secondary" className="text-sm">
                                        {equipment.name}
                                    </Badge>
                                ))}

                                {activity.activityEquipments.length > MAX_EQUIPMENT_VISIBLE && (
                                    <Badge variant="outline" className="text-sm bg-blue-200 text-blue-800">
                                        +{activity.activityEquipments.length - MAX_EQUIPMENT_VISIBLE}
                                    </Badge>
                                )}
                            </div>
                        </div>
                    </>
                )}
            </CardContent>
        </Card>
    );
}
