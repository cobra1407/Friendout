import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Euro, Gift, Plus } from "lucide-react";
import { getTranslation } from "@/i18n";
import type { ActivityDetails } from "../types/activityDetails.type";
import { cn } from "@/lib/utils"; // ← Assure-toi d'avoir cette fonction utilitaire shadcn

interface ActivityCostSummaryProps {
    activity: ActivityDetails;
    totalPrice: number;
}

export default function ActivityCostSummary({
    activity,
    totalPrice,
}: ActivityCostSummaryProps) {
    const subActivitiesWithPrice = activity.subActivities?.filter((sub) => sub?.price && Number(sub.price) > 0) || [];
    const mainPrice = Number(activity.estimatedPrice || 0);
    const subsPrice = subActivitiesWithPrice.reduce((sum, sub) => sum + Number(sub.price || 0), 0);
    const isFree = totalPrice <= 0;
    return (
        <Card
            className={cn(
                "overflow-hidden border transition-all duration-200 hover:shadow-md",
                isFree
                    ? "bg-gradient-to-br from-emerald-50/80 to-emerald-100/40 dark:from-emerald-950/30 dark:to-emerald-900/10 border-emerald-200/70 dark:border-emerald-800/50"
                    : "bg-card border-border"
            )}
        >
            <CardHeader className="pb-4">
                {!isFree && (
                    <CardTitle className="flex items-center gap-2.5 text-lg font-semibold">
                        <Euro className="h-5 w-5 text-emerald-600" />
                        {getTranslation("activity.cost_summary")}
                    </CardTitle>
                )}
            </CardHeader>

            <CardContent className="space-y-5 pb-6">
                {isFree ? (
                    <div className="flex flex-col items-center justify-center rounded-xl bg-card/60 p-8 text-center shadow-sm">
                        <div className="mb-4 rounded-full bg-emerald-500/15 p-4">
                            <Gift className="h-8 w-8 text-emerald-600 dark:text-emerald-400" />
                        </div>
                        <p className="text-xl font-semibold text-emerald-800 dark:text-emerald-300">
                            {getTranslation("activity.all_free")}
                        </p>
                        <p className="mt-2 text-sm text-muted-foreground max-w-md">
                            {getTranslation("activity.no_expense_planned")}
                        </p>
                    </div>
                ) : (
                    <>
                        {/* Breakdown */}
                        <div className="grid gap-4 sm:grid-cols-2">
                            {/* Main activity */}
                            <div className="group relative rounded-lg border bg-card/80 p-4 transition-all hover:border-emerald-300 dark:hover:border-emerald-700 hover:shadow-sm">
                                <div className="mb-1 text-xs font-medium uppercase tracking-wide text-muted-foreground">
                                    {getTranslation("activity.main_activity")}
                                </div>
                                <div
                                    className={cn(
                                        "text-2xl font-bold",
                                        mainPrice > 0 ? "text-emerald-700 dark:text-emerald-400" : "text-muted-foreground"
                                    )}
                                >
                                    {mainPrice > 0
                                        ? `${mainPrice.toFixed(2)} €`
                                        : getTranslation("common.free")}
                                </div>
                            </div>

                            {/* Sub activities */}
                            {(activity.subActivities?.length ?? 0) > 0 && (
                                <div className="group relative rounded-lg border bg-card/80 p-4 transition-all hover:border-emerald-300 dark:hover:border-emerald-700 hover:shadow-sm">
                                    <div className="mb-1 flex items-center gap-1.5 text-xs font-medium uppercase tracking-wide text-muted-foreground">
                                        <Plus className="h-3.5 w-3.5" />
                                        {getTranslation("activity.sub_activities_short")}
                                    </div>
                                    <div
                                        className={cn(
                                            "text-2xl font-bold",
                                            subsPrice > 0 ? "text-blue-600/90 dark:text-blue-400" : "text-muted-foreground"
                                        )}
                                    >
                                        <span className="text-emerald-600 dark:text-emerald-400"> {subsPrice > 0
                                            ? `${subsPrice.toFixed(2)} €`
                                            : getTranslation("common.free")}
                                        </span>
                                    </div>
                                </div>
                            )}
                        </div>

                        {/* Total */}
                        <div className="rounded-lg bg-emerald-500/10 px-5 py-4 ring-1 ring-emerald-500/30">
                            <div className="flex items-center justify-between">
                                <span className="text-base font-semibold text-emerald-800 dark:text-emerald-300">
                                    {getTranslation("activity.total")}
                                </span>

                                <span className="text-3xl font-extrabold text-emerald-700 dark:text-emerald-400">
                                    {totalPrice.toFixed(2)} €
                                </span>
                            </div>
                        </div>
                    </>
                )}
            </CardContent>
        </Card>
    );
}
