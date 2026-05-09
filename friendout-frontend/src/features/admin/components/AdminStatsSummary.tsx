import { Shield, Mail, Users, AlertCircle } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { getTranslation } from "@/i18n";
import { cn } from "@/lib/utils";
import { useAdminGuilds, useAdminEmails, useAdminUsers, useAdminAccessRequests } from "../hooks/useAdmin";

interface AdminStatsSummaryProps {
    onOpenRequests: () => void;
}

export const AdminStatsSummary = ({ onOpenRequests }: AdminStatsSummaryProps) => {
    const { users } = useAdminUsers();
    const { requests } = useAdminAccessRequests();
    const { emails } = useAdminEmails();
    const { guilds } = useAdminGuilds();

    const stats = [
        { label: getTranslation('admin.stats.users'), value: users.length, icon: Users, color: "text-blue-600", bg: "bg-blue-50 dark:bg-blue-950/40", highlight: false, onClick: undefined as (() => void) | undefined },
        { label: getTranslation('admin.stats.pending_requests'), value: requests.length, icon: AlertCircle, color: "text-amber-600", bg: "bg-amber-50 dark:bg-amber-950/40", highlight: requests.length > 0, onClick: onOpenRequests },
        { label: getTranslation('admin.stats.allowed_emails'), value: emails.length, icon: Mail, color: "text-emerald-600", bg: "bg-emerald-50 dark:bg-emerald-950/40", highlight: false, onClick: undefined as (() => void) | undefined },
        { label: getTranslation('admin.stats.discord_guilds'), value: guilds.length, icon: Shield, color: "text-indigo-600", bg: "bg-indigo-50 dark:bg-indigo-950/40", highlight: false, onClick: undefined as (() => void) | undefined },
    ];

    return (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
            {stats.map((stat) => (
                <Card
                    key={stat.label}
                    onClick={stat.onClick}
                    className={cn(
                        "border shadow-sm transition-all",
                        stat.onClick && "cursor-pointer hover:shadow-md",
                        stat.highlight && "ring-1 ring-amber-300 dark:ring-amber-700",
                    )}
                >
                    <CardContent className="p-4 flex items-center gap-3">
                        <div className={cn("p-2.5 rounded-xl shrink-0", stat.bg)}>
                            <stat.icon className={cn("w-5 h-5", stat.color)} />
                        </div>
                        <div className="min-w-0">
                            <p className="text-xs text-muted-foreground truncate">{stat.label}</p>
                            <div className="flex items-baseline gap-1.5">
                                <p className="text-2xl font-bold tracking-tight leading-none mt-0.5">
                                    {stat.value}
                                </p>
                                {stat.onClick && requests.length > 0 && (
                                    <span className="text-[10px] text-amber-600 font-semibold uppercase tracking-wide">
                                        {getTranslation('admin.stats.view')}
                                    </span>
                                )}
                            </div>
                        </div>
                    </CardContent>
                </Card>
            ))}
        </div>
    );
};
