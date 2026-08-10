import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router";
import {
    LogIn,
    Share2,
} from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { ErrorState } from "@/features/error/components/ErrorState";
import { getTranslation } from "@/i18n";
import { formatTime } from "@/lib/utils/date.utils";
import { resolveMediaUrl } from "@/lib/media";
import { useOgMeta } from "@/lib/utils/useOgMeta";
import { getPublicActivity } from "@/features/activity/api/activityShare.api";
import type { PublicActivity } from "@/features/activity/types/publicActivity.type";
import { Header } from "@/components/header";
import ActivityMainDetails from "@/features/activity/components/ActivityMainDetails";
import { PublicActivityParticipation } from "../components/PublicActitiyParticipation";
import { useAuth } from "@/features/auth/hooks/useAuth";

export default function PublicActivityPage() {
    const { shareToken } = useParams<{ shareToken: string }>();
    const navigate = useNavigate();
    const { isAuthenticated, loading: authLoading, fetchMe } = useAuth();

    const [activity, setActivity] = useState<PublicActivity | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [notFound, setNotFound] = useState(false);

    useEffect(() => {
        fetchMe();
    }, [fetchMe]);

    useEffect(() => {
        if (!shareToken) {
            setNotFound(true);
            setIsLoading(false);
            return;
        }

        let cancelled = false;

        getPublicActivity(shareToken)
            .then((data) => {
                if (!cancelled) setActivity(data);
            })
            .catch(() => {
                if (!cancelled) setNotFound(true);
            })
            .finally(() => {
                if (!cancelled) setIsLoading(false);
            });

        return () => {
            cancelled = true;
        };
    }, [shareToken]);

    useEffect(() => {
        if (!authLoading && isAuthenticated && activity?.activityId) {
            navigate(`/activities/${activity.activityId}`, { replace: true });
        }
    }, [authLoading, isAuthenticated, activity, navigate]);

    useOgMeta({
        title: activity?.title ?? "Friendout",
        description: activity?.description,
        imageUrl: resolveMediaUrl(activity?.image?.url),
    });

    const waitingForPrivateRedirect = !authLoading && isAuthenticated && !!activity;

    if (isLoading || authLoading || waitingForPrivateRedirect) {
        return (
            <div className="flex items-center justify-center min-h-screen bg-slate-50 dark:bg-background">
                <Spinner className="size-6 text-slate-600" />
            </div>
        );
    }

    if (notFound || !activity) {
        return (
            <ErrorState
                icon="🔗"
                title={getTranslation("public_activity_page.not_found_title")}
                description={getTranslation("public_activity_page.not_found_description")}
                primaryAction={{
                    label: getTranslation("error404.back_home"),
                    onClick: () => navigate("/"),
                }}
            />
        );
    }

    const pricedSubActivitiesCount = activity.subActivities.filter((sa) => !!sa.price).length;

    const totalPrice =
        (activity.estimatedPrice ?? 0) +
        activity.subActivities.reduce((sum, sa) => sum + (sa.price ?? 0), 0);

    const mainDetailsProps = {
        title: activity.title,
        description: activity.description || "Aucune description fournie.",
        startAt: activity.startAt,
        image: activity.image,
        localisation: activity.localisation,
        createdBy: activity.createdBy,
        price: {
            totalPrice,
            estimatedPrice: activity.estimatedPrice,
            pricedSubActivitiesCount,
        },
        equipmentNames: activity.requiredEquipmentNames,
        imageBadge: (
            <Badge
                variant="outline"
                className="text-xs bg-background/60 backdrop-blur-sm border-border/50 shadow-sm"
            >
                <Share2 className="w-3 h-3 mr-1" />
                {getTranslation("public_activity_page.footer_text")}
            </Badge>
        ),
    };

    const participatingCount = activity.participantsCount?.participating ?? 0;
    const maybeCount = activity.participantsCount?.maybe ?? 0;
    const totalParticipants = participatingCount + maybeCount;

    return (
        <div className="min-h-screen bg-slate-50 dark:bg-background text-foreground flex flex-col">
            <Header isPublicPage={true} />

            <main className="w-full max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-6 flex-1">

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">

                    <div className="lg:col-span-2 space-y-6 min-w-0">
                        <ActivityMainDetails {...mainDetailsProps} />

                        {activity.subActivities.length > 0 && (
                            <Card className="border-border/60 shadow-sm bg-white dark:bg-card rounded-2xl">
                                <CardHeader className="pb-3">
                                    <CardTitle className="text-base font-semibold text-slate-900 dark:text-slate-100">
                                        {getTranslation("public_activity_page.sub_activities_title")}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent className="space-y-2">
                                    {activity.subActivities.map((sa) => (
                                        <div key={sa.name} className="rounded-xl border border-slate-200 dark:border-slate-800 p-3.5 bg-slate-50/50 dark:bg-slate-900/30">
                                            <div className="flex items-center justify-between gap-2">
                                                <span className="font-medium text-sm text-slate-800 dark:text-slate-200">{sa.name}</span>
                                                <span className="text-xs text-slate-500">
                                                    {formatTime(sa.startTime)} - {formatTime(sa.endTime)}
                                                </span>
                                            </div>
                                            {sa.description && (
                                                <p className="text-xs text-slate-500 mt-1">
                                                    {sa.description}
                                                </p>
                                            )}
                                        </div>
                                    ))}
                                </CardContent>
                            </Card>
                        )}
                    </div>

                    <div className="space-y-6">
                        <PublicActivityParticipation
                            totalParticipants={totalParticipants}
                            participatingCount={participatingCount}
                            maybeCount={maybeCount}
                        />

                        <Card className="border-emerald-200 dark:border-emerald-900 bg-emerald-50/50 dark:bg-emerald-950/20 rounded-2xl p-5 space-y-3">
                            <div className="space-y-1">
                                <p className="font-semibold text-sm text-slate-900 dark:text-slate-100">
                                    {getTranslation("public_activity_page.login_prompt_title")}
                                </p>
                                <p className="text-xs text-slate-600 dark:text-slate-400">
                                    {getTranslation("public_activity_page.login_prompt_description")}
                                </p>
                            </div>
                            <Button
                                onClick={() => navigate("/login")}
                                className="w-full bg-slate-900 hover:bg-slate-800 text-white gap-2 shadow-sm"
                            >
                                <LogIn className="w-4 h-4" />
                                {getTranslation("public_activity_page.login_button")}
                            </Button>
                        </Card>

                    </div>
                </div>
            </main>
        </div>
    );
}
