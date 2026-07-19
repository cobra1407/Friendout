import { ShieldCheck } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { getTranslation } from "@/i18n";
import { useHealthCheck } from "@/features/admin/hooks/useAdmin";

const STATUS_STYLES = {
    checking: "text-muted-foreground border-muted-foreground/30 bg-muted/40",
    healthy: "text-emerald-600 border-emerald-200 bg-emerald-50 dark:bg-emerald-950/30 dark:border-emerald-800 dark:text-emerald-400",
    down: "text-red-600 border-red-200 bg-red-50 dark:bg-red-950/30 dark:border-red-800 dark:text-red-400",
} as const;

const STATUS_DOT_STYLES = {
    checking: "bg-muted-foreground/50",
    healthy: "bg-emerald-500 animate-pulse",
    down: "bg-red-500 animate-pulse",
} as const;

const STATUS_LABEL_KEYS = {
    checking: "admin.system_checking",
    healthy: "admin.system_operational",
    down: "admin.system_down",
} as const;

export const AdminPageHeader = () => {
    const { status } = useHealthCheck();

    return (
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pt-2 overflow-x-hidden">
            <div className="space-y-0.5">
                <p className="flex items-center gap-1.5 text-xs font-semibold text-primary uppercase tracking-widest">
                    <ShieldCheck className="w-3.5 h-3.5" />
                    Administration
                </p>
                <h1 className="text-2xl font-bold tracking-tight">{getTranslation('admin.page_title')}</h1>
            </div>
            {/*
                status is null for the first ~400ms while the health check is still
                in flight (see useHealthCheck) — we render nothing during that window
                on purpose. The check almost always resolves faster than that, so the
                badge appears once, already in its final state, instead of flashing
                from a neutral "checking" look to green/red.
            */}
            {status && (
                <Badge
                    variant="outline"
                    className={`self-start sm:self-auto flex items-center gap-1.5 px-3 py-1.5 ${STATUS_STYLES[status]}`}
                >
                    <span className={`w-1.5 h-1.5 rounded-full ${STATUS_DOT_STYLES[status]}`} />
                    {getTranslation(STATUS_LABEL_KEYS[status])}
                </Badge>
            )}
        </div>
    );
};
