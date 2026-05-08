import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminApi } from "../api/admin.api";
import { toast } from "sonner";

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
            toast.success("Serveur ajouté");
            setGuildId("");
            setLabel("");
            qc.invalidateQueries({ queryKey: ["admin", "guilds"] });
        },
        onError: () => toast.error("Ce serveur existe déjà ou une erreur est survenue"),
    });

    const deleteMutation = useMutation({
        mutationFn: adminApi.deleteGuild,
        onSuccess: () => {
            toast.success("Serveur supprimé");
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
            toast.success("Email ajouté");
            setEmail("");
            qc.invalidateQueries({ queryKey: ["admin", "emails"] });
        },
        onError: () => toast.error("Cet email existe déjà ou une erreur est survenue"),
    });

    const deleteMutation = useMutation({
        mutationFn: adminApi.deleteEmail,
        onSuccess: () => {
            toast.success("Email supprimé");
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
            toast.success("Rôle mis à jour");
            qc.invalidateQueries({ queryKey: ["admin", "users"] });
        },
        onError: () => toast.error("Erreur lors de la mise à jour du rôle"),
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
            toast.success(vars.status === "Approved" ? "Demande approuvée" : "Demande refusée");
            qc.invalidateQueries({ queryKey: ["admin", "access-requests"] });
        },
        onError: () => toast.error("Erreur lors du traitement de la demande"),
    });

    return { requests, isLoading, resolveMutation };
};
