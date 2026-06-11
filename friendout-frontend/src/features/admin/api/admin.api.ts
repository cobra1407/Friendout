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
    message: string | null;
    status: "Pending" | "Approved" | "Denied";
    createdAt: string;
    resolvedAt: string | null;
}

export interface AccessSettingsDto {
    discordRestricted: boolean;
    googleRestricted: boolean;
}

export interface AccessModeDto {
    isOpenMode: boolean;
    guildCount: number;
    emailCount: number;
}

export interface AppLogDto {
    id: number;
    level: "Info" | "Warning" | "Error";
    category: string;
    message: string;
    exception: string | null;
    createdAt: string;
}

export const adminApi = {

    // ------ Logs management ------
    getLogs: (level?: string, limit = 200) =>
        api.get<AppLogDto[]>("/admin/logs", { params: { level, limit } }).then(r => r.data),
    clearLogs: () => api.delete("/admin/logs"),
    exportLogs: () => api.get("/admin/logs/export", { responseType: "blob" }).then(r => r.data as Blob),

    // ------ Guilds management ------
    getGuilds: () => api.get<GuildDto[]>("/admin/allowed-guilds").then(r => r.data),
    addGuild: (guildId: string, label?: string) =>
        api.post<GuildDto>("/admin/allowed-guilds", { guildId, label }).then(r => r.data),
    deleteGuild: (id: number) => api.delete(`/admin/allowed-guilds/${id}`),

    // ------ Emails management ------
    getEmails: () => api.get<EmailDto[]>("/admin/allowed-emails").then(r => r.data),
    addEmail: (email: string) => api.post<EmailDto>("/admin/allowed-emails", { email }).then(r => r.data),
    deleteEmail: (id: number) => api.delete(`/admin/allowed-emails/${id}`),

    // ------ Users management ------
    getUsers: () => api.get<UserAdminDto[]>("/admin/users").then(r => r.data),
    updateUserRole: (id: string, role: UserRole) =>
        api.put<UserAdminDto>(`/admin/users/${id}/role`, { role }).then(r => r.data),
    deleteUser: (id: string) => api.delete(`/admin/users/${id}`),

    // ------ Access settings ------
    getAccessSettings: () => api.get<AccessSettingsDto>("/admin/access-settings").then(r => r.data),
    updateAccessSettings: (dto: AccessSettingsDto) =>
        api.put<AccessSettingsDto>("/admin/access-settings", dto).then(r => r.data),

    // ------ Access requests management ------
    getAccessMode: () => api.get<AccessModeDto>("/admin/access-mode").then(r => r.data),

    getAccessRequests: (status?: string) =>
        api.get<AccessRequestDto[]>("/admin/access-requests", { params: { status } }).then(r => r.data),
    resolveAccessRequest: (id: number, status: "Approved" | "Denied") =>
        api.put<AccessRequestDto>(`/admin/access-requests/${id}`, { status }).then(r => r.data),

    submitAccessRequest: (dto: { email: string; message?: string }) =>
        api.post("/access-requests", dto),
};
