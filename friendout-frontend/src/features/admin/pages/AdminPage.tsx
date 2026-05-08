import {
    Shield,
    Server,
    Mail,
    Users,
    Check,
    Trash2,
    Plus,
    Search,
    AlertCircle,
    MoreHorizontal,
    ShieldCheck,
    X,
} from "lucide-react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
    CardDescription,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
// Remplace les imports Dialog
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal"

import { Spinner } from "@/components/ui/spinner";
import { Header } from "@/components/header";
import { ActivityLayout } from "@/features/activity/layout/activityLayout";
import { authApi } from "@/features/auth/api/auth.api";
import {
    useAdminGuilds,
    useAdminEmails,
    useAdminUsers,
    useAdminAccessRequests,
} from "../hooks/useAdmin";
import { getTranslation } from "@/i18n";
import { cn } from "@/lib/utils";

export default function AdminPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [requestsOpen, setRequestsOpen] = useState(false);

    useEffect(() => {
        if (user && user.role !== "Admin") navigate("/activities");
    }, [user, navigate]);

    if (!user || user.role !== "Admin") return null;

    const handleLogout = async () => {
        await authApi.logout();
        navigate("/login");
    };

    return (
        <ActivityLayout
            header={
                <Header
                    onCreateActivity={() => navigate("/activities/createActivity")}
                    onLogout={handleLogout}
                />
            }
        >
            <div className="max-w-7xl mx-auto w-full pb-10 space-y-6 px-4">
                <PageHeader />
                <StatsSummary onOpenRequests={() => setRequestsOpen(true)} />

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <div className="lg:col-span-2">
                        <UsersSection />
                    </div>
                    <div className="flex flex-col gap-6">
                        <GuildsSection />
                        <EmailsSection />
                    </div>
                </div>
            </div>

            <AccessRequestsModal
                open={requestsOpen}
                onClose={() => setRequestsOpen(false)}
            />

        </ActivityLayout>
    );
}

const PageHeader = () => {
    return (
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
}

const StatsSummary = ({ onOpenRequests }: { onOpenRequests: () => void }) => {
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
}


const AccessRequestsModal = ({ open, onClose }: { open: boolean; onClose: () => void }) => {
    const { requests, isLoading, resolveMutation } = useAdminAccessRequests();

    return (
        <Modal open={open} onClose={onClose} className="max-w-lg">
            <ModalHeader>
                <ModalTitle className="flex items-center gap-2">
                    <AlertCircle className="w-4 h-4 text-amber-600" />
                    {getTranslation('admin.requests.modal_title')}
                    {requests.length > 0 && (
                        <Badge className="bg-amber-100 text-amber-700 border-amber-200 ml-1">
                            {requests.length}
                        </Badge>
                    )}
                </ModalTitle>
                <ModalDescription>
                    {getTranslation('admin.requests.modal_description')}
                </ModalDescription>
            </ModalHeader>

            <div className="mt-2">
                {isLoading ? (
                    <div className="flex justify-center py-8"><Spinner /></div>
                ) : requests.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-10 text-muted-foreground">
                        <Check className="w-10 h-10 mb-2 opacity-20" />
                        <p className="text-sm italic">{getTranslation('admin.requests.empty')}</p>
                    </div>
                ) : (
                    <ul className="divide-y max-h-[60vh] overflow-y-auto">
                        {requests.map((r) => (
                            <li key={r.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 py-3 first:pt-0">
                                <div className="flex items-center gap-3">
                                    <div className="h-9 w-9 rounded-full bg-muted flex items-center justify-center text-sm font-semibold shrink-0">
                                        {r.name?.[0]?.toUpperCase() ?? "?"}
                                    </div>
                                    <div>
                                        <p className="text-sm font-medium">{r.name ?? getTranslation('admin.requests.unknown_name')}</p>
                                        <p className="text-xs text-muted-foreground">{r.email}</p>
                                        {r.message && (
                                            <p className="text-xs text-muted-foreground italic mt-0.5">"{r.message}"</p>
                                        )}
                                    </div>
                                </div>
                                <div className="flex items-center gap-2 shrink-0">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="h-8 text-xs text-destructive border-destructive/20 hover:bg-destructive/5"
                                        disabled={resolveMutation.isPending}
                                        onClick={() => resolveMutation.mutate({ id: r.id, status: "Denied" })}
                                    >
                                        <X className="w-3.5 h-3.5 mr-1" /> {getTranslation('admin.requests.deny')}
                                    </Button>
                                    <Button
                                        size="sm"
                                        className="h-8 text-xs"
                                        disabled={resolveMutation.isPending}
                                        onClick={() => resolveMutation.mutate({ id: r.id, status: "Approved" })}
                                    >
                                        <Check className="w-3.5 h-3.5 mr-1" /> {getTranslation('admin.requests.approve')}
                                    </Button>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </Modal>
    );
}


const UsersSection = () => {
    const { users, isLoading, updateRoleMutation } = useAdminUsers();
    const [search, setSearch] = useState("");

    const filteredUsers = users.filter(
        (u) =>
            u.name.toLowerCase().includes(search.toLowerCase()) ||
            u.email?.toLowerCase().includes(search.toLowerCase())
    );

    return (
        <Card className="border shadow-sm h-full">
            <CardHeader className="pb-3">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                    <div className="flex items-center gap-2">
                        <div className="p-1.5 rounded-lg bg-blue-50 dark:bg-blue-950/40">
                            <Users className="w-4 h-4 text-blue-600" />
                        </div>
                        <div>
                            <CardTitle className="text-base">{getTranslation('admin.users.title')}</CardTitle>
                            <CardDescription className="text-xs">{getTranslation('admin.users.description')}</CardDescription>
                        </div>
                    </div>
                    <div className="relative">
                        <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground" />
                        <Input
                            placeholder={getTranslation('admin.search_placeholder')}
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            className="pl-8 h-8 text-sm w-full sm:w-52"
                        />
                    </div>
                </div>
            </CardHeader>
            <CardContent className="pt-0 max-h-200 overflow-y-auto">
                {isLoading ? (
                    <div className="flex justify-center py-8"><Spinner /></div>
                ) : filteredUsers.length === 0 ? (
                    <p className="text-sm text-muted-foreground text-center py-8 italic">{getTranslation('admin.no_results')}</p>
                ) : (
                    <ul className="divide-y">
                        {filteredUsers.map((u) => (
                            <li key={u.id} className="flex items-center justify-between gap-3 py-3 first:pt-0 last:pb-0">
                                <div className="flex items-center gap-3 min-w-0">
                                    <Avatar className="h-8 w-8 shrink-0">
                                        <AvatarImage src={u.avatarUrl ?? undefined} />
                                        <AvatarFallback className="text-xs font-semibold">
                                            {u.name.slice(0, 2).toUpperCase()}
                                        </AvatarFallback>
                                    </Avatar>
                                    <div className="min-w-0">
                                        <p className="text-sm font-medium truncate">{u.name}</p>
                                        <p className="text-xs text-muted-foreground truncate">{u.email}</p>
                                    </div>
                                </div>
                                <div className="flex items-center gap-2 shrink-0">
                                    <Badge
                                        variant="secondary"
                                        className={cn(
                                            "text-xs",
                                            u.role === "Admin"
                                                ? "bg-red-50 text-red-600 dark:bg-red-950/40 dark:text-red-400"
                                                : "bg-blue-50 text-blue-600 dark:bg-blue-950/40 dark:text-blue-400"
                                        )}
                                    >
                                        {u.role}
                                    </Badge>
                                    <DropdownMenu modal={false}>
                                        <DropdownMenuTrigger asChild>
                                            <Button variant="ghost" size="icon" className="h-7 w-7">
                                                <MoreHorizontal className="w-4 h-4" />
                                            </Button>
                                        </DropdownMenuTrigger>
                                        <DropdownMenuContent align="end" className="w-48">
                                            <DropdownMenuItem
                                                disabled={u.role === "User" || updateRoleMutation.isPending}
                                                onClick={() => updateRoleMutation.mutate({ id: u.id, role: "User" })}
                                            >
                                                {getTranslation('admin.users.set_user')}
                                            </DropdownMenuItem>
                                            <DropdownMenuItem
                                                disabled={u.role === "Admin" || updateRoleMutation.isPending}
                                                className="text-destructive"
                                                onClick={() => updateRoleMutation.mutate({ id: u.id, role: "Admin" })}
                                            >
                                                {getTranslation('admin.users.set_admin')}
                                            </DropdownMenuItem>
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </CardContent>
        </Card>
    );
}

// ─────────────────────────────────────────────────────────────────────────────
// Guilds
// ─────────────────────────────────────────────────────────────────────────────

const GuildsSection = () => {
    const { guilds, isLoading, guildId, setGuildId, label, setLabel, addMutation, deleteMutation } =
        useAdminGuilds();
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
                {/* Add form */}
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

                {/* Search — only shown when there are enough guilds to warrant it */}
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

                {/* List */}
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
}

// ─────────────────────────────────────────────────────────────────────────────
// Emails
// ─────────────────────────────────────────────────────────────────────────────

const EmailsSection = () => {
    const { emails, isLoading, email, setEmail, addMutation, deleteMutation } = useAdminEmails();

    return (
        <Card className="border shadow-sm">
            <CardHeader className="pb-3">
                <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-lg bg-emerald-50 dark:bg-emerald-950/40">
                        <Mail className="w-4 h-4 text-emerald-600" />
                    </div>
                    <div>
                        <CardTitle className="text-base">{getTranslation('admin.emails.title')}</CardTitle>
                        <CardDescription className="text-xs">{getTranslation('admin.emails.description')}</CardDescription>
                    </div>
                </div>
            </CardHeader>
            <CardContent className="pt-0 space-y-3">
                <div className="flex gap-2">
                    <Input
                        type="email"
                        placeholder="prenom@gmail.com"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="h-8 text-sm"
                    />
                    <Button
                        size="sm"
                        className="h-8 shrink-0"
                        disabled={!email.trim() || addMutation.isPending}
                        onClick={() => addMutation.mutate()}
                    >
                        <Plus className="w-3.5 h-3.5" />
                    </Button>
                </div>

                {isLoading ? (
                    <div className="flex justify-center py-3"><Spinner /></div>
                ) : emails.length === 0 ? (
                    <p className="text-xs text-muted-foreground text-center py-3 italic">{getTranslation('admin.emails.empty')}</p>
                ) : (
                    <ul className="space-y-1.5 max-h-64 overflow-y-auto">
                        {emails.map((e) => (
                            <li key={e.id} className="flex items-center justify-between gap-2 px-3 py-2 rounded-lg bg-muted/40 hover:bg-muted/60 transition-colors">
                                <span className="text-sm truncate">{e.email}</span>
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="h-6 w-6 shrink-0 text-muted-foreground hover:text-destructive"
                                    disabled={deleteMutation.isPending}
                                    onClick={() => deleteMutation.mutate(e.id)}
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
}
