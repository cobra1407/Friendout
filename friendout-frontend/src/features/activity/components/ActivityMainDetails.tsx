import type { ReactNode } from "react";
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

import type { Image } from "@/features/activity/types/image.type";
import type { Localisation } from "@/features/localisation/types/localisation.type";
import { LocalisationType } from "@/features/localisation/types/localisation.type";

import { isPast, formatDate, formatTime } from "@/lib/utils/date.utils";
import { Badge } from "@/components/ui/badge";
import { getTranslation } from "@/i18n";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { resolveMediaUrl } from "@/lib/media";

import {
    normalizeLocalisation,
    getGoogleMapsUrl,
} from "@/features/localisation/utils/localisation.utils";

export interface ActivityPriceInfo {
    totalPrice?: number | null;
    estimatedPrice?: number | null;
    pricedSubActivitiesCount?: number;
}

export type ActivityMainDetailsProps = {
    title: string;
    description: string;
    startAt: string;
    image?: Image | null;
    localisation?: Localisation | null;
    createdBy: string;
    price: ActivityPriceInfo;
    equipmentNames?: string[];
    maxEquipmentVisible?: number;
    imageBadge?: ReactNode;
};

export default function ActivityMainDetails({
    title,
    description,
    startAt,
    image,
    localisation: localisationProp,
    createdBy,
    price,
    equipmentNames = [],
    maxEquipmentVisible,
    imageBadge,
}: ActivityMainDetailsProps) {

    const localisation = normalizeLocalisation(localisationProp);
    const isGoogleMapsLink = localisation?.type === LocalisationType.MapLink;


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
                    <CardTitle className="text-2xl">{title}</CardTitle>
                    {isPast(startAt) && (
                        <Badge variant="secondary">{getTranslation("activity.past")}</Badge>
                    )}
                </div>
            </CardHeader>

            <CardContent className="space-y-6">
                <div className="w-full h-64 rounded-lg overflow-hidden relative">
                    <img
                        src={resolveMediaUrl(image?.url) ?? DefaultActivityImage}
                        alt={title}
                    />
                    {imageBadge && (
                        <div className="absolute top-3 right-3">
                            {imageBadge}
                        </div>
                    )}
                </div>

                <div className="space-y-4">
                    <div className="flex items-center gap-2">
                        <Calendar className="w-5 h-5 text-blue-600" />
                        <span className="font-medium">{formatDate(startAt)}</span>
                    </div>

                    <div className="flex items-center gap-2">
                        <Clock className="w-4 h-4 text-green-600" />
                        <span>{formatTime(startAt)}</span>
                    </div>

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

                        <span className="flex-1" title={localisation?.address ?? ""}>
                            {localisation?.address ?? "-"}
                        </span>

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

                    <div className="flex items-center gap-3">
                        {price.totalPrice && price.totalPrice > 0 ? (
                            <>
                                <Euro className="w-5 h-5 text-yellow-600" />
                                <div className="flex flex-col">
                                    <span className="font-medium text-lg text-yellow-600">
                                        {getTranslation("activity.total")}:{" "}
                                        {Number(price.totalPrice).toFixed(2)}€
                                    </span>

                                    {!!price.pricedSubActivitiesCount && (
                                        <div className="text-sm text-muted-foreground">
                                            <span>
                                                {getTranslation("activity.main_activity_label")}{" "}
                                                {price.estimatedPrice && price.estimatedPrice > 0 ? (
                                                    <span className="font-bold text-muted-foreground">
                                                        {price.estimatedPrice.toFixed(2)}€
                                                    </span>
                                                ) : (
                                                    getTranslation("common.free")
                                                )}
                                            </span>
                                            {price.pricedSubActivitiesCount > 0 && (
                                                <span className="ml-2">
                                                    +{" "}
                                                    {price.pricedSubActivitiesCount}{" "}
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

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <Users className="w-3 h-3 text-muted-foreground mt-1" />
                            <span className="text-xs text-muted-foreground mt-1">
                                {getTranslation("activity.created_by")}{" "}
                                <span className="font-medium">{createdBy}</span>
                            </span>
                        </div>
                    </div>
                </div>

                <Separator />

                <div>
                    <h3 className="font-semibold mb-3">
                        {getTranslation("activity.description")}
                    </h3>
                    <Linkify options={linkifyOptions}>
                        <p className="text-foreground whitespace-pre-wrap">
                            {description}
                        </p>
                    </Linkify>
                </div>

                {equipmentNames.length > 0 && (
                    <>
                        <Separator />
                        <div>
                            <h3 className="font-semibold mb-3 flex items-center gap-2">
                                <Package className="w-5 h-5" />
                                {getTranslation("activity.necessary_equipment")}
                            </h3>
                            <div className="flex flex-wrap gap-2">
                                {equipmentNames
                                    .slice(0, MAX_EQUIPMENT_VISIBLE)
                                    .map((name, idx) => (
                                        <Badge
                                            key={idx}
                                            variant="secondary"
                                            className="text-sm"
                                        >
                                            {name}
                                        </Badge>
                                    ))}

                                {equipmentNames.length > MAX_EQUIPMENT_VISIBLE && (
                                    <Badge
                                        variant="outline"
                                        className="text-sm bg-blue-500/15 text-blue-700 dark:text-blue-400 border-none"
                                    >
                                        +{equipmentNames.length - MAX_EQUIPMENT_VISIBLE}
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
