import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { getTranslation } from "@/i18n";
import { CheckCircle2, HelpCircle, Users } from "lucide-react"

interface PublicActivityParticipationProps {
    totalParticipants: number;
    participatingCount: number;
    maybeCount: number;
}

export const PublicActivityParticipation = ({ totalParticipants, participatingCount, maybeCount }: PublicActivityParticipationProps) => {
    return (
        <Card className="border-border/60 shadow-sm bg-white dark:bg-card rounded-2xl">
            <CardHeader className="pb-3">
                <CardTitle className="text-base font-semibold flex items-center gap-2 text-slate-900 dark:text-slate-100">
                    <Users className="w-4 h-4 text-slate-500" />
                    {getTranslation("public_activity_page.participants_title_count", { count: totalParticipants })}
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
                {totalParticipants === 0 ? (
                    <p className="text-xs text-slate-500">
                        {getTranslation("public_activity_page.participants_none")}
                    </p>
                ) : (
                    <div className="space-y-2">
                        {participatingCount > 0 && (
                            <div className="flex items-center gap-2 p-2.5 rounded-lg bg-emerald-50 dark:bg-emerald-950/30 text-emerald-800 dark:text-emerald-300 text-xs border border-emerald-100 dark:border-emerald-900/40">
                                <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />
                                <span className="font-medium">
                                    {getTranslation(
                                        participatingCount === 1
                                            ? "public_activity_page.participants_confirmed_sentence_one"
                                            : "public_activity_page.participants_confirmed_sentence",
                                        { count: participatingCount }
                                    )}
                                </span>
                            </div>
                        )}

                        {maybeCount > 0 && (
                            <div className="flex items-center gap-2 p-2.5 rounded-lg bg-amber-50 dark:bg-amber-950/30 text-amber-800 dark:text-amber-300 text-xs border border-amber-100 dark:border-amber-900/40">
                                <HelpCircle className="w-4 h-4 text-amber-600 shrink-0" />
                                <span className="font-medium">
                                    {getTranslation(
                                        maybeCount === 1
                                            ? "public_activity_page.participants_maybe_sentence_one"
                                            : "public_activity_page.participants_maybe_sentence",
                                        { count: maybeCount }
                                    )}
                                </span>
                            </div>
                        )}
                    </div>
                )}
            </CardContent>
        </Card>
    )
}
