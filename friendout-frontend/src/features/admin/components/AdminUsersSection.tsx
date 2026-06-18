import { Users, Search, MoreHorizontal, ShieldOff, Trash2 } from "lucide-react";
import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { cn } from "@/lib/utils";
import { UserRole } from "@/features/user/enum/userRole.enum";
import { useAdminUsers } from "../hooks/useAdmin";

interface PendingDemotion {
    id: string;
    name: string;
}

export const AdminUsersSection = () => {
    const { users, isLoading, isFetchingNextPage, usersLoaderRef, updateRoleMutation, deleteUserMutation } = useAdminUsers();
    const [search, setSearch] = useState("");
    const [pendingDemotion, setPendingDemotion] = useState<PendingDemotion | null>(null);
    const [pendingDelete, setPendingDelete] = useState<PendingDemotion | null>(null);

    const filteredUsers = users.filter(
        (u) =>
            u.name.toLowerCase().includes(search.toLowerCase()) ||
            u.email?.toLowerCase().includes(search.toLowerCase())
    );

    const handleDemoteClick = (id: string, name: string) => {
        setPendingDemotion({ id, name });
    };

    const handleDemoteConfirm = () => {
        if (!pendingDemotion) return;
        updateRoleMutation.mutate(
            { id: pendingDemotion.id, role: UserRole.User },
            { onSettled: () => setPendingDemotion(null) }
        );
    };

    return (
        <>
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
                        <>
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
                                                    u.role === UserRole.Admin
                                                        ? "bg-red-50 text-red-600 dark:bg-red-950/40 dark:text-red-400"
                                                        : "bg-blue-50 text-blue-600 dark:bg-blue-950/40 dark:text-blue-400"
                                                )}
                                            >
                                                {getTranslation(`admin.roles.${u.role.toLowerCase()}`)}
                                            </Badge>
                                            <DropdownMenu modal={false}>
                                                <DropdownMenuTrigger asChild>
                                                    <Button variant="ghost" size="icon" className="h-7 w-7">
                                                        <MoreHorizontal className="w-4 h-4" />
                                                    </Button>
                                                </DropdownMenuTrigger>
                                                <DropdownMenuContent align="end" className="w-48">
                                                    <DropdownMenuItem
                                                        disabled={u.role === UserRole.User || updateRoleMutation.isPending}
                                                        onClick={() => handleDemoteClick(u.id, u.name)}
                                                    >
                                                        {getTranslation('admin.users.set_user')}
                                                    </DropdownMenuItem>
                                                    <DropdownMenuItem
                                                        disabled={u.role === UserRole.Admin || updateRoleMutation.isPending}
                                                        className="text-destructive"
                                                        onClick={() => updateRoleMutation.mutate({ id: u.id, role: UserRole.Admin })}
                                                    >
                                                        {getTranslation('admin.users.set_admin')}
                                                    </DropdownMenuItem>
                                                    <DropdownMenuItem
                                                        disabled={deleteUserMutation.isPending}
                                                        className="text-destructive"
                                                        onClick={() => setPendingDelete({ id: u.id, name: u.name })}
                                                    >
                                                        {getTranslation('admin.users.delete_user')}
                                                    </DropdownMenuItem>
                                                </DropdownMenuContent>
                                            </DropdownMenu>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                            {/* Infinite scroll sentinel */}
                            <div ref={usersLoaderRef} className="h-1" />
                            {isFetchingNextPage && (
                                <div className="flex justify-center py-2"><Spinner /></div>
                            )}
                        </>
                    )}
                </CardContent>
            </Card>

            <Modal
                open={!!pendingDemotion}
                onClose={() => setPendingDemotion(null)}
                className="max-w-sm"
            >
                <ModalHeader>
                    <div className="flex items-center gap-2">
                        <div className="p-2 rounded-full bg-amber-50 dark:bg-amber-950/40">
                            <ShieldOff className="w-4 h-4 text-amber-600" />
                        </div>
                        <ModalTitle>
                            {getTranslation('admin.users.demote_confirm_title', { name: pendingDemotion?.name ?? '' })}
                        </ModalTitle>
                    </div>
                    <ModalDescription>
                        {getTranslation('admin.users.demote_confirm_description', { name: pendingDemotion?.name ?? '' })}
                    </ModalDescription>
                </ModalHeader>
                <div className="flex justify-end gap-2 mt-4">
                    <Button
                        variant="outline"
                        onClick={() => setPendingDemotion(null)}
                        disabled={updateRoleMutation.isPending}
                    >
                        {getTranslation('admin.users.cancel')}
                    </Button>
                    <Button
                        variant="destructive"
                        onClick={handleDemoteConfirm}
                        disabled={updateRoleMutation.isPending}
                    >
                        {updateRoleMutation.isPending
                            ? <Spinner className="w-4 h-4" />
                            : getTranslation('admin.users.demote_confirm')
                        }
                    </Button>
                </div>
            </Modal>

            <Modal
                open={!!pendingDelete}
                onClose={() => setPendingDelete(null)}
                className="max-w-sm"
            >
                <ModalHeader>
                    <div className="flex items-center gap-2">
                        <div className="p-2 rounded-full bg-red-50 dark:bg-red-950/40">
                            <Trash2 className="w-4 h-4 text-destructive" />
                        </div>
                        <ModalTitle>
                            {getTranslation('admin.users.delete_confirm_title', { name: pendingDelete?.name ?? '' })}
                        </ModalTitle>
                    </div>
                    <ModalDescription>
                        {getTranslation('admin.users.delete_confirm_description', { name: pendingDelete?.name ?? '' })}
                    </ModalDescription>
                </ModalHeader>
                <div className="flex justify-end gap-2 mt-4">
                    <Button
                        variant="outline"
                        onClick={() => setPendingDelete(null)}
                        disabled={deleteUserMutation.isPending}
                    >
                        {getTranslation('admin.users.cancel')}
                    </Button>
                    <Button
                        variant="destructive"
                        onClick={() => {
                            if (!pendingDelete) return;
                            deleteUserMutation.mutate(pendingDelete.id, {
                                onSettled: () => setPendingDelete(null)
                            });
                        }}
                        disabled={deleteUserMutation.isPending}
                    >
                        {deleteUserMutation.isPending
                            ? <Spinner className="w-4 h-4" />
                            : getTranslation('admin.users.delete_confirm')
                        }
                    </Button>
                </div>
            </Modal>
        </>
    );
};
