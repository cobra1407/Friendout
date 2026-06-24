import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { getTranslation } from "@/i18n";
import { Calendar, Clock, Euro, MapPin, Users } from "lucide-react";
import defaultActivityImage from "@/assets/images/default-activity-card.png"

interface ThemeLivePreviewProps {
    className?: string;
}

const ThemeLivePreview = ({ className }: ThemeLivePreviewProps) => {
    return (
        <div className={`flex flex-col align-items center gap-2 ${className || ''}`}>

            <p className="text-xs font-medium text-muted-foreground mb-2">
                {getTranslation("preferences.theme.preview_label")}
            </p>
            <Card className="max-w-[340px]">
                <CardHeader className="pb-3">
                    <div className="flex flex-wrap gap-2 mb-1">
                        <Badge variant="outline" className="text-xs bg-blue-500/15 text-blue-700 dark:text-blue-400 border-none">
                            {getTranslation("preferences.theme.preview_badge")}
                        </Badge>
                    </div>
                    <CardTitle className="text-base font-semibold">
                        {getTranslation("preferences.theme.preview_activity_title")}
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <div className="relative w-full h-28 rounded-md overflow-hidden">
                        <img
                            src={defaultActivityImage}
                            alt=""
                            className="absolute inset-0 w-full h-full object-cover"
                        />
                    </div>
                    <p className="text-xs text-muted-foreground line-clamp-2">
                        {getTranslation("preferences.theme.preview_description")}
                    </p>
                    <div className="space-y-1.5 text-xs">
                        <div className="flex items-center gap-2">
                            <Calendar className="w-3.5 h-3.5 text-blue-600" />
                            <span>{getTranslation("preferences.theme.preview_date")}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <Clock className="w-3.5 h-3.5 text-green-600" />
                            <span>{getTranslation("preferences.theme.preview_time")}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <MapPin className="w-3.5 h-3.5 text-red-700" />
                            <span>{getTranslation("preferences.theme.preview_location")}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <Euro className="w-3.5 h-3.5 text-green-600" />
                            <span className="font-medium text-green-600">
                                {getTranslation("preferences.theme.preview_price")}
                            </span>
                        </div>
                        <div className="flex items-center gap-2">
                            <Users className="w-3.5 h-3.5 text-purple-600" />
                            <span>{getTranslation("preferences.theme.preview_participants")}</span>
                        </div>
                    </div>
                </CardContent>
                <CardFooter className="flex gap-2">
                    <Button size="sm" className="flex-1">
                        {getTranslation("preferences.theme.preview_button_view_details")}
                    </Button>
                </CardFooter>
            </Card>
        </div>
    );
};

export default ThemeLivePreview;
