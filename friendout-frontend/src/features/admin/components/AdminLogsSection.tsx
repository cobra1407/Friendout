import { ScrollText, Trash2, Download, Filter, Check, MoreHorizontal } from "lucide-react";
import { useState, type ReactNode } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { getTranslation } from "@/i18n";
import { useAdminLogs } from "../hooks/useAdmin";
import { adminApi, type AppLogDto } from "../api/admin.api";
import { cn } from "@/lib/utils";

const ROLE_STYLES: Record<string, string> = {
    Admin: "font-semibold text-red-600 dark:text-red-400",
    User: "font-semibold text-blue-600 dark:text-blue-400",
};

/** Scans the message for known role values and wraps them in colored spans. */
const renderMessage = (message: string): ReactNode => {
    const parts = message.split(/(\bAdmin\b|\bUser\b)/g);
    if (parts.length === 1) return <span>{message}</span>;
    return (
        <>
            {parts.map((part, i) =>
                ROLE_STYLES[part]
                    ? <span key={i} className={ROLE_STYLES[part]}>{part}</span>
                    : <span key={i}>{part}</span>
            )}
        </>
    );
};

const LEVEL_BADGE: Record<AppLogDto["level"], string> = {
    Info: "bg-blue-50 text-blue-700 dark:bg-blue-950/40 dark:text-blue-300",
    Warning: "bg-amber-50 text-amber-700 dark:bg-amber-950/40 dark:text-amber-300",
    Error: "bg-red-50 text-red-700 dark:bg-red-950/40 dark:text-red-300",
};

const LEVELS: AppLogDto["level"][] = ["Info", "Warning", "Error"];

export const AdminLogsSection = () => {
    const { logs, isLoading, levelFilter, setLevelFilter, clearMutation } = useAdminLogs();
    const [filterOpen, setFilterOpen] = useState(false);

    const handleExport = async () => {
        const blob = await adminApi.exportLogs();
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `friendout-logs-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        URL.revokeObjectURL(url);
    };

    const setLevelAndClose = (level?: AppLogDto["level"]) => {
        setLevelFilter(level);
        setFilterOpen(false);
    };

    return (
        <Card className="border shadow-sm">
            <CardHeader className="pb-3">
                <div className="relative flex flex-col sm:flex-row sm:items-center justify-between gap-2">
                    <div className="flex items-center gap-2">
                        <div className="p-1.5 rounded-lg bg-slate-100 dark:bg-slate-800">
                            <ScrollText className="w-4 h-4 text-slate-600 dark:text-slate-400" />
                        </div>
                        <div>
                            <CardTitle className="text-base">{getTranslation("admin.logs.title")}</CardTitle>
                            <CardDescription className="text-xs py-1">{getTranslation("admin.logs.description")}</CardDescription>
                        </div>
                    </div>

                    <div className="absolute top-0 right-0 flex items-center gap-1.5 sm:static sm:shrink-0">
                        {/* Filter by level only on small screens */}
                        <Popover open={filterOpen}>
                            <PopoverTrigger asChild>
                                <Button
                                    variant="outline" size="sm"
                                    className={cn("h-7 text-xs cursor-pointer gap-1", levelFilter && "border-primary text-primary")}
                                    onClick={() => setFilterOpen(p => !p)}
                                >
                                    <Filter className="w-3 h-3" />
                                    <span className="hidden sm:inline">{levelFilter ?? getTranslation("admin.logs.filter_all")}</span>
                                </Button>
                            </PopoverTrigger>
                            <PopoverContent align="end" className="w-40 p-1">
                                <button
                                    onClick={() => setLevelAndClose(undefined)}
                                    className="flex w-full items-center gap-2 rounded px-2 py-1.5 text-sm hover:bg-muted transition-colors cursor-pointer"
                                >
                                    <Check className={cn("w-3.5 h-3.5", levelFilter ? "invisible" : "visible")} />
                                    {getTranslation("admin.logs.filter_all")}
                                </button>
                                {LEVELS.map(level => (
                                    <button
                                        key={level}
                                        onClick={() => setLevelAndClose(levelFilter === level ? undefined : level)}
                                        className="flex w-full items-center gap-2 rounded px-2 py-1.5 text-sm hover:bg-muted transition-colors cursor-pointer"
                                    >
                                        <Check className={cn("w-3.5 h-3.5", levelFilter === level ? "visible" : "invisible")} />
                                        {level}
                                    </button>
                                ))}
                            </PopoverContent>
                        </Popover>

                        {/* only on large screens */}
                        <div className="sm:hidden">
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="outline" size="sm" className="h-7 cursor-pointer">
                                        <MoreHorizontal className="w-4 h-4" />
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end" className="w-48">
                                    <DropdownMenuItem
                                        onClick={handleExport}
                                        disabled={logs.length === 0}
                                        className="flex items-center gap-2 cursor-pointer"
                                    >
                                        <Download className="w-3.5 h-3.5" />
                                        {getTranslation("admin.logs.export")}
                                    </DropdownMenuItem>
                                    <DropdownMenuItem
                                        onClick={() => clearMutation.mutate()}
                                        disabled={clearMutation.isPending || logs.length === 0}
                                        className="flex items-center gap-2 text-destructive focus:text-destructive cursor-pointer"
                                    >
                                        <Trash2 className="w-3.5 h-3.5" />
                                        {getTranslation("admin.logs.clear")}
                                    </DropdownMenuItem>
                                </DropdownMenuContent>
                            </DropdownMenu>
                        </div>

                        <Button
                            variant="outline" size="sm" className="hidden sm:flex h-7 text-xs cursor-pointer"
                            onClick={handleExport} disabled={logs.length === 0}
                            title={getTranslation("admin.logs.export")}
                        >
                            <Download className="w-3.5 h-3.5" />
                        </Button>

                        <Button
                            variant="outline" size="sm"
                            className="hidden sm:flex h-7 text-xs text-destructive hover:text-destructive cursor-pointer"
                            onClick={() => clearMutation.mutate()}
                            disabled={clearMutation.isPending || logs.length === 0}
                            title={getTranslation("admin.logs.clear")}
                        >
                            <Trash2 className="w-3.5 h-3.5" />
                        </Button>
                    </div>
                </div>
            </CardHeader>

            <CardContent className="pt-0">
                {isLoading ? (
                    <div className="flex justify-center py-6"><Spinner /></div>
                ) : logs.length === 0 ? (
                    <p className="text-xs text-muted-foreground text-center py-6 italic">
                        {getTranslation("admin.logs.empty")}
                    </p>
                ) : (
                    <ul className="space-y-1 max-h-[32rem] overflow-y-auto pr-0.5">
                        {logs.map(log => (
                            <li
                                key={log.id}
                                className="flex items-start gap-2 px-3 py-2 rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors"
                            >
                                <span className={`shrink-0 mt-0.5 px-1.5 py-0.5 rounded text-[10px] font-semibold uppercase tracking-wide ${LEVEL_BADGE[log.level]}`}>
                                    {log.level}
                                </span>
                                <div className="min-w-0 flex-1">
                                    <p className="text-xs text-foreground break-words">{renderMessage(log.message)}</p>
                                    <div className="flex items-center gap-2 mt-0.5">
                                        <span className="text-[10px] text-muted-foreground font-mono">{log.category}</span>
                                        <span className="text-[10px] text-muted-foreground">
                                            {new Date(log.createdAt).toLocaleString()}
                                        </span>
                                    </div>
                                    {log.exception && (
                                        <pre className="mt-1 text-[10px] text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-950/30 rounded p-1.5 overflow-x-auto whitespace-pre-wrap break-all">
                                            {log.exception}
                                        </pre>
                                    )}
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </CardContent>
        </Card>
    );
};
