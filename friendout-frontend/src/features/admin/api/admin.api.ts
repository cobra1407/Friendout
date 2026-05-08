import type { UserRole } from "@/features/user/enum/userRole.enum";
import api from "@/lib/api/api";


export interface GuildDto {
    id: number;
    guildId: string;
    label: string | null;
    createdAt: string;
}

export interface EmailDto {
    id: number;
    email: string;
    createdAt: string;
}

export interface UserAdminDto {
    id: string;
    name: string;
    email: string | null;
    avatarUrl: string | null;
    role: UserRole;
    createdAt: string;
}

export interface AccessRequestDto {
    id: number;
    email: string;
    name: string | null;
    message: string | null;
    status: "Pending" | "Approved" | "Denied";
    createdAt: string;
    resolvedAt: string | null;
}

export const adminApi = {
    getGuilds: () => api.get<GuildDto[]>("/admin/allowed-guilds").then(r => r.data),
    addGuild: (guildId: string, label?: string) =>
        api.post<GuildDto>("/admin/allowed-guilds", { guildId, label }).then(r => r.data),
    deleteGuild: (id: number) => api.delete(`/admin/allowed-guilds/${id}`),

    getEmails: () => api.get<EmailDto[]>("/admin/allowed-emails").then(r => r.data),
    addEmail: (email: string) =>
        api.post<EmailDto>("/admin/allowed-emails", { email }).then(r => r.data),
    deleteEmail: (id: number) => api.delete(`/admin/allowed-emails/${id}`),

    getUsers: () => api.get<UserAdminDto[]>("/admin/users").then(r => r.data),

    updateUserRole: (id: string, role: UserRole) =>
        api.put<UserAdminDto>(`/admin/users/${id}/role`, { role }).then(r => r.data),

    getAccessRequests: (status?: string) =>
        api.get<AccessRequestDto[]>("/admin/access-requests", { params: { status } }).then(r => r.data),
    resolveAccessRequest: (id: number, status: "Approved" | "Denied") =>
        api.put<AccessRequestDto>(`/admin/access-requests/${id}`, { status }).then(r => r.data),
};
