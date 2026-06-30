import { Server, Search, Plus, Trash2, TriangleAlert, InfoIcon } from "lucide-react";
import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal";
import { getTranslation } from "@/i18n";
import { useAdminGuilds, useAccessMode } from "../hooks/useAdmin";

export const AdminGuildsSection = () => {
    const { guilds, isLoading, guildId, setGuildId, label, setLabel, addMutation, deleteMutation } = useAdminGuilds();
    const { accessMode } = useAccessMode();
    const [search, setSearch] = useState("");
    const [pendingDeleteId, setPendingDeleteId] = useState<number | null>(null);
    const pendingGuild = guilds.find(g => g.id === pendingDeleteId);

    const filteredGuilds = guilds.filter(
        (g) =>
            g.guildId.toLowerCase().includes(search.toLowerCase()) ||
            g.label?.toLowerCase().includes(search.toLowerCase())
    );

    const handleAddKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter" && guildId.trim() && !addMutation.isPending) {
            addMutation.mutate();
        }
    };

    return (
        <>
            <Card className="border shadow-sm">
                <CardHeader className="pb-3">
                    <div className="flex items-center gap-2">
                        <div className="p-1.5 rounded-lg bg-indigo-50 dark:bg-indigo-950/40">
                            <Server className="w-4 h-4 text-indigo-600" />
                        </div>
                        <div>
                            <CardTitle className="text-base">{getTranslation('admin.guilds.title')}</CardTitle>
                            <CardDescription className="text-xs">{getTranslation('admin.guilds.description')}</CardDescription>
                        </div>
                    </div>
                </CardHeader>
                <CardContent className="pt-0 space-y-3">
                    {accessMode?.isDiscordOpenMode && (
                        <div className="flex items-start gap-2 rounded-lg border border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-950/40 px-3 py-2.5">
                            <TriangleAlert className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
                            <p className="text-xs text-amber-800 dark:text-amber-300">
                                {getTranslation('admin.guilds.open_mode_warning')}
                            </p>
                        </div>
                    )}
                    {!accessMode?.isDiscordOpenMode && accessMode?.isDiscordRestrictionLocksEveryone && !accessMode?.noLoginMethodAvailable && (
                        <div className="flex items-start gap-2 rounded-lg border border-sky-200 bg-sky-50 dark:border-sky-800 dark:bg-sky-950/40 px-3 py-2.5">
                            <InfoIcon className="w-4 h-4 text-sky-600 dark:text-sky-400 shrink-0 mt-0.5" />
                            <p className="text-xs text-sky-800 dark:text-sky-300">
                                {getTranslation('admin.guilds.disabled_as_login_method_info')}
                            </p>
                        </div>
                    )}
                    {accessMode?.noLoginMethodAvailable && (
                        <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-950/40 px-3 py-2.5">
                            <TriangleAlert className="w-4 h-4 text-red-600 dark:text-red-400 shrink-0 mt-0.5" />
                            <p className="text-xs text-red-800 dark:text-red-300">
                                {getTranslation('admin.guilds.no_login_method_warning')}
                            </p>
                        </div>
                    )}
                    <div className="flex flex-col gap-2">
                        <Input
                            placeholder={getTranslation('admin.guilds.guild_id_placeholder')}
                            value={guildId}
                            onChange={(e) => setGuildId(e.target.value)}
                            onKeyDown={handleAddKeyDown}
                            className="h-8 text-sm font-mono"
                        />
                        <div className="flex gap-2">
                            <Input
                                placeholder={getTranslation('admin.guilds.label_placeholder')}
                                value={label}
                                onChange={(e) => setLabel(e.target.value)}
                                onKeyDown={handleAddKeyDown}
                                className="h-8 text-sm"
                            />
                            <Button
                                size="sm"
                                className="h-8 shrink-0"
                                disabled={!guildId.trim() || addMutation.isPending}
                                onClick={() => addMutation.mutate()}
                            >
                                <Plus className="w-3.5 h-3.5" />
                            </Button>
                        </div>
                    </div>

                    {guilds.length > 4 && (
                        <div className="relative">
                            <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground" />
                            <Input
                                placeholder={getTranslation('admin.search_placeholder')}
                                value={search}
                                onChange={(e) => setSearch(e.target.value)}
                                className="pl-8 h-8 text-sm"
                            />
                        </div>
                    )}

                    {isLoading ? (
                        <div className="flex justify-center py-3"><Spinner /></div>
                    ) : filteredGuilds.length === 0 ? (
                        <p className="text-xs text-muted-foreground text-center py-3 italic">
                            {search ? getTranslation('admin.no_results') : getTranslation('admin.guilds.empty')}
                        </p>
                    ) : (
                        <ul className="space-y-1.5 max-h-64 overflow-y-auto pr-0.5">
                            {filteredGuilds.map((g) => (
                                <li key={g.id} className="flex items-center justify-between gap-2 px-3 py-2 rounded-lg bg-muted/40 hover:bg-muted/60 transition-colors">
                                    <div className="min-w-0">
                                        <p className="text-sm font-medium truncate">{g.label ?? getTranslation('admin.guilds.no_name')}</p>
                                        <p className="text-[10px] font-mono text-muted-foreground truncate">{g.guildId}</p>
                                    </div>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-6 w-6 shrink-0 text-muted-foreground hover:text-destructive"
                                        disabled={deleteMutation.isPending}
                                        onClick={() => setPendingDeleteId(g.id)}
                                    >
                                        <Trash2 className="w-3 h-3" />
                                    </Button>
                                </li>
                            ))}
                        </ul>
                    )}
                </CardContent>
            </Card>

            <Modal
                open={pendingDeleteId !== null}
                onClose={() => setPendingDeleteId(null)}
                className="max-w-sm"
            >
                <ModalHeader>
                    <div className="flex items-center gap-2">
                        <div className="p-2 rounded-full bg-red-50 dark:bg-red-950/40">
                            <Trash2 className="w-4 h-4 text-destructive" />
                        </div>
                        <ModalTitle>
                            {getTranslation('admin.guilds.delete_confirm_title')}
                        </ModalTitle>
                    </div>
                    <ModalDescription>
                        {getTranslation('admin.guilds.delete_confirm_description', {
                            name: pendingGuild?.label ?? pendingGuild?.guildId ?? ''
                        })}
                    </ModalDescription>
                </ModalHeader>
                <div className="flex justify-end gap-2 mt-4">
                    <Button
                        variant="outline"
                        onClick={() => setPendingDeleteId(null)}
                        disabled={deleteMutation.isPending}
                    >
                        {getTranslation('admin.users.cancel')}
                    </Button>
                    <Button
                        variant="destructive"
                        disabled={deleteMutation.isPending}
                        onClick={() => {
                            if (pendingDeleteId !== null) {
                                deleteMutation.mutate(pendingDeleteId, {
                                    onSettled: () => setPendingDeleteId(null)
                                });
                            }
                        }}
                    >
                        {deleteMutation.isPending
                            ? <Spinner className="w-4 h-4" />
                            : getTranslation('admin.guilds.delete_confirm')
                        }
                    </Button>
                </div>
            </Modal>
        </>
    );
};
