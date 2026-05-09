import { Users, Search, MoreHorizontal } from "lucide-react";
import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { cn } from "@/lib/utils";
import { UserRole } from "@/features/user/enum/userRole.enum";
import { useAdminUsers } from "../hooks/useAdmin";

export const AdminUsersSection = () => {
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
                                                onClick={() => updateRoleMutation.mutate({ id: u.id, role: UserRole.User })}>
                                                {getTranslation('admin.users.set_user')}
                                            </DropdownMenuItem>
                                            <DropdownMenuItem
                                                disabled={u.role === UserRole.Admin || updateRoleMutation.isPending}
                                                className="text-destructive"
                                                onClick={() => updateRoleMutation.mutate({ id: u.id, role: UserRole.Admin })}>
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
};
