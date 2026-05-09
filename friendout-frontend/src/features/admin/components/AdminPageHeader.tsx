import { ShieldCheck } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { getTranslation } from "@/i18n";

export const AdminPageHeader = () => (
    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pt-2 overflow-x-hidden">
        <div className="space-y-0.5">
            <p className="flex items-center gap-1.5 text-xs font-semibold text-primary uppercase tracking-widest">
                <ShieldCheck className="w-3.5 h-3.5" />
                Administration
            </p>
            <h1 className="text-2xl font-bold tracking-tight">{getTranslation('admin.page_title')}</h1>
        </div>
        <Badge
            variant="outline"
            className="self-start sm:self-auto flex items-center gap-1.5 px-3 py-1.5 text-emerald-600 border-emerald-200 bg-emerald-50 dark:bg-emerald-950/30 dark:border-emerald-800 dark:text-emerald-400"
        >
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
            {getTranslation('admin.system_operational')}
        </Badge>
    </div>
);
