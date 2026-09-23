import { useState } from "react";
import { z } from "zod";
import { ShieldAlert, Send, TriangleAlert } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { Checkbox } from "@/components/ui/checkbox";
import { FieldError } from "@/components/ui/FieldError";
import { getTranslation } from "@/i18n";
import { accountDeletionApi } from "@/features/auth/api/accountDeletion.api";

const emailSchema = z.string().min(1).email();

interface AccountDeletionRequestModalProps {
    open: boolean;
    onClose: () => void;
    defaultEmail?: string;
}

export const AccountDeletionRequestModal = ({ open, onClose, defaultEmail = "" }: AccountDeletionRequestModalProps) => {
    const [email, setEmail] = useState(defaultEmail);
    const [deleteCreatedActivities, setDeleteCreatedActivities] = useState(false);
    const [error, setError] = useState<string | undefined>();
    const [submitted, setSubmitted] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const handleClose = () => {
        setEmail("");
        setDeleteCreatedActivities(false);
        setError(undefined);
        setSubmitted(false);
        onClose();
    };

    const handleSubmit = async () => {
        const parsed = emailSchema.safeParse(email.trim());
        if (!parsed.success) {
            setError(getTranslation("account_deletion.error_email_invalid"));
            return;
        }

        setError(undefined);
        setIsLoading(true);

        try {
            // The backend always returns success regardless of whether the email
            // matches an account — nothing to branch on here.
            await accountDeletionApi.requestDeletion(parsed.data, deleteCreatedActivities);
            setSubmitted(true);
        } catch {
            setError(getTranslation("account_deletion.error_generic"));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <Modal open={open} onClose={handleClose} className="max-w-md">
            {submitted ? (
                <div className="flex flex-col items-center gap-4 py-8 px-2 text-center">
                    <div className="h-16 w-16 rounded-full bg-primary/10 flex items-center justify-center">
                        <ShieldAlert className="w-8 h-8 text-primary" />
                    </div>
                    <div className="space-y-1">
                        <p className="text-lg font-semibold">{getTranslation("account_deletion.request_success_title")}</p>
                        <p className="text-sm text-muted-foreground leading-relaxed">
                            {getTranslation("account_deletion.request_success_description")}
                        </p>
                    </div>
                    <Button className="mt-2" onClick={handleClose}>
                        {getTranslation("common.close")}
                    </Button>
                </div>
            ) : (
                <div className="space-y-5">
                    <div>
                        <h2 className="text-lg font-semibold">{getTranslation("account_deletion.request_modal_title")}</h2>
                        <p className="text-sm text-muted-foreground mt-1 leading-relaxed">
                            {getTranslation("account_deletion.request_modal_description")}
                        </p>
                    </div>

                    <div className="space-y-1">
                        <p className="text-sm font-medium">{getTranslation("account_deletion.email_label")}</p>
                        <Input
                            id="deletion-email"
                            type="email"
                            value={email}
                            onChange={(e) => { setEmail(e.target.value); setError(undefined); }}
                            placeholder={getTranslation("account_deletion.email_placeholder")}
                            aria-invalid={!!error}
                            className={error ? "border-destructive focus-visible:ring-destructive" : ""}
                        />
                        <FieldError message={error} />
                    </div>

                    <div className="flex items-start gap-2">
                        <Checkbox
                            id="delete-created-activities"
                            checked={deleteCreatedActivities}
                            onCheckedChange={(checked) => setDeleteCreatedActivities(checked === true)}
                            className="mt-0.5"
                        />
                        <label htmlFor="delete-created-activities" className="text-sm leading-snug cursor-pointer">
                            {getTranslation("account_deletion.delete_activities_checkbox")}
                        </label>
                    </div>

                    {deleteCreatedActivities && (
                        <div className="flex items-start gap-2 rounded-md border border-destructive/30 bg-destructive/5 p-3">
                            <TriangleAlert className="w-4 h-4 text-destructive mt-0.5 shrink-0" />
                            <p className="text-sm text-destructive leading-snug">
                                {getTranslation("account_deletion.delete_activities_warning")}
                            </p>
                        </div>
                    )}

                    <div className="flex justify-end gap-2 pt-1">
                        <Button variant="ghost" onClick={handleClose} disabled={isLoading}>
                            {getTranslation("common.cancel")}
                        </Button>
                        <Button variant="destructive" onClick={handleSubmit} disabled={isLoading}>
                            <Send className="w-4 h-4 mr-2" />
                            {isLoading
                                ? getTranslation("account_deletion.submitting")
                                : getTranslation("account_deletion.submit_button")}
                        </Button>
                    </div>
                </div>
            )}
        </Modal>
    );
};
