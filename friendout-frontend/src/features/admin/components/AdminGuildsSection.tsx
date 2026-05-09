import { Server, Search, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { useAdminGuilds } from "../hooks/useAdmin";

export const AdminGuildsSection = () => {
    const { guilds, isLoading, guildId, setGuildId, label, setLabel, addMutation, deleteMutation } = useAdminGuilds();
    const [search, setSearch] = useState("");

    const filteredGuilds = guilds.filter(
        (g) =>
            g.guildId.toLowerCase().includes(search.toLowerCase()) ||
            g.label?.toLowerCase().includes(search.toLowerCase())
    );

    return (
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
                <div className="flex flex-col gap-2">
                    <Input
                        placeholder={getTranslation('admin.guilds.guild_id_placeholder')}
                        value={guildId}
                        onChange={(e) => setGuildId(e.target.value)}
                        className="h-8 text-sm font-mono"
                    />
                    <div className="flex gap-2">
                        <Input
                            placeholder={getTranslation('admin.guilds.label_placeholder')}
                            value={label}
                            onChange={(e) => setLabel(e.target.value)}
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
                                    onClick={() => deleteMutation.mutate(g.id)}
                                >
                                    <Trash2 className="w-3 h-3" />
                                </Button>
                            </li>
                        ))}
                    </ul>
                )}
            </CardContent>
        </Card>
    );
};
