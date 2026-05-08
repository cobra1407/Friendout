import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminApi } from "../api/admin.api";
import { toast } from "sonner";
import { getTranslation } from "@/i18n";

export const useAdminGuilds = () => {
    const qc = useQueryClient();
    const [guildId, setGuildId] = useState("");
    const [label, setLabel] = useState("");

    const { data: guilds = [], isLoading } = useQuery({
        queryKey: ["admin", "guilds"],
        queryFn: adminApi.getGuilds,
    });

    const addMutation = useMutation({
        mutationFn: () => adminApi.addGuild(guildId.trim(), label.trim() || undefined),
        onSuccess: () => {
            toast.success(getTranslation('admin.toast.guild_added'));
            setGuildId("");
            setLabel("");
            qc.invalidateQueries({ queryKey: ["admin", "guilds"] });
        },
        onError: () => toast.error(getTranslation('admin.toast.guild_error')),
    });

    const deleteMutation = useMutation({
        mutationFn: adminApi.deleteGuild,
        onSuccess: () => {
            toast.success(getTranslation('admin.toast.guild_deleted'));
            qc.invalidateQueries({ queryKey: ["admin", "guilds"] });
        },
    });

    return { guilds, isLoading, guildId, setGuildId, label, setLabel, addMutation, deleteMutation };
};

export const useAdminEmails = () => {
    const qc = useQueryClient();
    const [email, setEmail] = useState("");

    const { data: emails = [], isLoading } = useQuery({
        queryKey: ["admin", "emails"],
        queryFn: adminApi.getEmails,
    });

    const addMutation = useMutation({
        mutationFn: () => adminApi.addEmail(email.trim()),
        onSuccess: () => {
            toast.success(getTranslation('admin.toast.email_added'));
            setEmail("");
            qc.invalidateQueries({ queryKey: ["admin", "emails"] });
        },
        onError: () => toast.error(getTranslation('admin.toast.email_error')),
    });

    const deleteMutation = useMutation({
        mutationFn: adminApi.deleteEmail,
        onSuccess: () => {
            toast.success(getTranslation('admin.toast.email_deleted'));
            qc.invalidateQueries({ queryKey: ["admin", "emails"] });
        },
    });

    return { emails, isLoading, email, setEmail, addMutation, deleteMutation };
};

export const useAdminUsers = () => {
    const qc = useQueryClient();

    const { data: users = [], isLoading } = useQuery({
        queryKey: ["admin", "users"],
        queryFn: adminApi.getUsers,
    });

    const updateRoleMutation = useMutation({
        mutationFn: ({ id, role }: { id: string; role: "Admin" | "User" }) =>
            adminApi.updateUserRole(id, role),
        onSuccess: () => {
            toast.success(getTranslation('admin.toast.role_updated'));
            qc.invalidateQueries({ queryKey: ["admin", "users"] });
        },
        onError: () => toast.error(getTranslation('admin.toast.role_error')),
    });

    return { users, isLoading, updateRoleMutation };
};

export const useAdminAccessRequests = () => {
    const qc = useQueryClient();

    const { data: requests = [], isLoading } = useQuery({
        queryKey: ["admin", "access-requests", "Pending"],
        queryFn: () => adminApi.getAccessRequests("Pending"),
    });

    const resolveMutation = useMutation({
        mutationFn: ({ id, status }: { id: number; status: "Approved" | "Denied" }) =>
            adminApi.resolveAccessRequest(id, status),
        onSuccess: (_, vars) => {
            toast.success(
                vars.status === "Approved"
                    ? getTranslation('admin.toast.request_approved')
                    : getTranslation('admin.toast.request_denied')
            );
            qc.invalidateQueries({ queryKey: ["admin", "access-requests"] });
        },
        onError: () => toast.error(getTranslation('admin.toast.request_error')),
    });

    return { requests, isLoading, resolveMutation };
};
