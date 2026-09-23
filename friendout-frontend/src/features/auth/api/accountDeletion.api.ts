import api from "@/lib/api/api";

export const accountDeletionApi = {
    /**
     * Starts the out-of-band account deletion flow for the given email.
     * Always resolves (202) regardless of whether the email matches an account —
     * the backend never reveals which addresses are registered.
     *
     * @param deleteCreatedActivities If true, every activity the user created is deleted
     * outright, even ones other people are participating in. If false (default), those
     * activities are orphaned instead (kept intact for other participants, ownerless).
     */
    requestDeletion: (email: string, deleteCreatedActivities: boolean = false) =>
        api.post("/account-deletion/request", { email, deleteCreatedActivities }),

    /**
     * Confirms deletion using the token from the confirmation email.
     * Rejects if the token is invalid or expired.
     */
    confirmDeletion: (token: string) =>
        api.post("/account-deletion/confirm", { token }),
};
