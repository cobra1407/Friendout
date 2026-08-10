import {
    Calendar,
    Clock,
    Euro,
    ExternalLink,
    Link,
    MapPin,
    MessageCircle,
    Package,
    Users,
} from "lucide-react";

import DefaultActivityImage from "@/assets/images/default-activity-card.webp";
import Linkify from "linkify-react";

import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import { LocalisationType } from "@/features/localisation/types/localisation.type";

import { isPast, formatDate, formatTime } from "@/lib/utils/date.utils";
import { Badge } from "@/components/ui/badge";
import { getTranslation } from "@/i18n";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { resolveMediaUrl } from "@/lib/media";

import {
    pickLocalisation,
    getGoogleMapsUrl,
} from "@/features/localisation/utils/localisation.utils";

type ActivityMainDetailsProps = {
    activity: ActivityDetails;
    maxEquipmentVisible?: number;
};

/**
 * Displays the main details of an activity.
 * @param {ActivityDetails} activity - The activity's details.
 * @param {number} maxEquipmentVisible - The maximum number of equipment to display.
 * @returns {JSX.Element} The main details of the activity.
 */
export default function ActivityMainDetails({
    activity,
    maxEquipmentVisible,
}: ActivityMainDetailsProps) {

    const localisation = pickLocalisation(activity);
    const isGoogleMapsLink = localisation?.type === LocalisationType.MapLink;


    /**
    * Opens the Google Maps page associated with the activity's location.
    * @remarks
    * Uses the `getGoogleMapsUrl` function to generate the URL of the Google Maps page.
    */
    const openInMaps = () => {
        const mapsUrl = getGoogleMapsUrl(localisation);
        window.open(mapsUrl, "_blank", "noopener,noreferrer");
    };

    const linkifyOptions = {
        target: "_blank",
        rel: "noopener noreferrer",
        className: "text-blue-700 dark:text-blue-400 hover:underline",
    };

    const MAX_EQUIPMENT_VISIBLE = maxEquipmentVisible ?? 10;

    return (
        <Card>
            <CardHeader>
                <div className="flex justify-between items-start">
                    <CardTitle className="text-2xl">{activity.title}</CardTitle>
                    {isPast(activity.startAt) && (
                        <Badge variant="secondary">{getTranslation("activity.past")}</Badge>
                    )}
                </div>
            </CardHeader>

            <CardContent className="space-y-6">
                {/* IMAGE */}
                <div className="w-full h-64 rounded-lg overflow-hidden relative">
                    <img
                        src={resolveMediaUrl(activity.image?.url) ?? DefaultActivityImage}
                        alt={activity.title}
                    />
                </div>

                {/* ––––––  DETAILS –––––– */}
                <div className="space-y-4">
                    {/* Date */}
                    <div className="flex items-center gap-2">
                        <Calendar className="w-5 h-5 text-blue-600" />
                        <span className="font-medium">{formatDate(activity.startAt)}</span>
                    </div>

                    {/* Time */}
                    <div className="flex items-center gap-2">
                        <Clock className="w-4 h-4 text-green-600" />
                        <span>{formatTime(activity.startAt)}</span>
                    </div>

                    {/* Localisation */}
                    <div className="flex items-center gap-2">
                        <div className="flex items-center gap-1">
                            {localisation?.type === LocalisationType.Virtual ? (
                                <>
                                    <MessageCircle className="w-5 h-5 text-blue-600" />
                                    <span>{localisation?.displayName}</span>
                                </>
                            ) : (
                                <>
                                    <MapPin className="w-5 h-5 text-red-600" />
                                    {isGoogleMapsLink && <Link className="w-4 h-4 text-blue-600" />}
                                </>
                            )}
                        </div>

                        {/* Displayed address – always visible even if empty */}
                        <span className="flex-1" title={localisation?.address ?? ""}>
                            {localisation?.address ?? "-"}
                        </span>

                        {/* Google Maps button – only for physical locations */}
                        {localisation?.type === LocalisationType.Virtual ? (
                            <Badge variant="outline">
                                {getTranslation("activity.virtual_place")}
                            </Badge>
                        ) : (
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={openInMaps}
                                className="flex items-center gap-1"
                                title={
                                    isGoogleMapsLink
                                        ? getTranslation("activity.open_google_maps")
                                        : getTranslation("activity.view_google_maps")
                                }
                                aria-label={getTranslation("activity.open_google_maps")}
                            >
                                <ExternalLink className="w-4 h-4" />
                                {isGoogleMapsLink
                                    ? getTranslation("activity.open")
                                    : getTranslation("activity.maps")}
                            </Button>
                        )}
                    </div>

                    {/* total price / free */}
                    <div className="flex items-center gap-3">
                        {activity.totalPrice && activity.totalPrice > 0 ? (
                            <>
                                <Euro className="w-5 h-5 text-yellow-600" />
                                <div className="flex flex-col">
                                    <span className="font-medium text-lg text-yellow-600">
                                        {getTranslation("activity.total")}:{" "}
                                        {Number(activity.totalPrice).toFixed(2)}€
                                    </span>

                                    {/* estimated price + sub activities */}
                                    {activity.subActivities?.some((s) => s.price) && (
                                        <div className="text-sm text-muted-foreground">
                                            <span>
                                                {getTranslation("activity.main_activity_label")}{" "}
                                                {activity.estimatedPrice && activity.estimatedPrice > 0 ? (
                                                    <span className="font-bold text-muted-foreground">
                                                        {activity.estimatedPrice.toFixed(2)}€
                                                    </span>
                                                ) : (
                                                    getTranslation("common.free")
                                                )}
                                            </span>
                                            {activity.subActivities.filter((s) => s.price).length > 0 && (
                                                <span className="ml-2">
                                                    +{" "}
                                                    {activity.subActivities.filter((s) => s.price).length}{" "}
                                                    {getTranslation("activity.sub_activities_count")}
                                                </span>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </>
                        ) : (
                            <>
                                <Euro className="w-5 h-5 text-green-600" />
                                <span className="font-medium text-lg text-green-600">
                                    {getTranslation("common.free")}
                                </span>
                            </>
                        )}
                    </div>

                    {/* Creator */}
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <Users className="w-3 h-3 text-muted-foreground mt-1" />
                            <span className="text-xs text-muted-foreground mt-1">
                                {getTranslation("activity.created_by")}{" "}
                                <span className="font-medium">{activity.createdBy}</span>
                            </span>
                        </div>
                    </div>
                </div>

                {/* ––––––  SEPARATOR –––––– */}
                <Separator />

                {/* ––––––  DESCRIPTION –––––– */}
                <div>
                    <h3 className="font-semibold mb-3">
                        {getTranslation("activity.description")}
                    </h3>
                    <Linkify options={linkifyOptions}>
                        <p className="text-foreground whitespace-pre-wrap break-words">
                            {activity.description}
                        </p>
                    </Linkify>
                </div>

                {/* ––––––  EQUIPMENTS –––––– */}
                {Array.isArray(activity.activityEquipments) &&
                    activity.activityEquipments.length > 0 && (
                        <>
                            <Separator />
                            <div>
                                <h3 className="font-semibold mb-3 flex items-center gap-2">
                                    <Package className="w-5 h-5" />
                                    {getTranslation("activity.necessary_equipment")}
                                </h3>
                                <div className="flex flex-wrap gap-2">
                                    {activity.activityEquipments
                                        .slice(0, MAX_EQUIPMENT_VISIBLE)
                                        .map((equipment, idx) => (
                                            <Badge
                                                key={equipment.equipmentId || idx}
                                                variant="secondary"
                                                className="text-sm"
                                            >
                                                {equipment.name}
                                            </Badge>
                                        ))}

                                    {activity.activityEquipments.length > MAX_EQUIPMENT_VISIBLE && (
                                        <Badge
                                            variant="outline"
                                            className="text-sm bg-blue-500/15 text-blue-700 dark:text-blue-400 border-none"
                                        >
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
