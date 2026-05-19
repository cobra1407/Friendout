import { useState } from "react";
import axios from "axios";
import { z } from "zod";
import { PartyPopper, Send } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Modal } from "@/components/ui/modal";
import { FieldError } from "@/components/ui/FieldError";
import { getTranslation } from "@/i18n";
import { adminApi } from "@/features/admin/api/admin.api";

const accessRequestSchema = z.object({
    email: z.string().min(1).email(),
    message: z.string().optional(),
});

type FieldErrors = { email?: string; api?: string };

interface RequestAccessModalProps {
    open: boolean;
    onClose: () => void;
}

export const RequestAccessModal = ({ open, onClose }: RequestAccessModalProps) => {
    const [email, setEmail]         = useState("");
    const [message, setMessage]     = useState("");
    const [honeypot, setHoneypot]   = useState("");
    const [errors, setErrors]       = useState<FieldErrors>({});
    const [submitted, setSubmitted] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const handleClose = () => {
        setEmail("");
        setMessage("");
        setHoneypot("");
        setErrors({});
        setSubmitted(false);
        onClose();
    };

    const handleSubmit = async () => {
        // Honeypot: bots fill hidden fields, humans don't.
        // Silently succeed to avoid revealing the check.
        if (honeypot) {
            setSubmitted(true);
            return;
        }

        const parsed = accessRequestSchema.safeParse({ email: email.trim(), message: message.trim() || undefined });

        if (!parsed.success) {
            const emailIssue = parsed.error.issues.find((i) => i.path[0] === "email");
            setErrors({
                email: emailIssue?.code === "too_small"
                    ? getTranslation("access_request.error_email_required")
                    : getTranslation("access_request.error_email_invalid"),
            });
            return;
        }

        setErrors({});
        setIsLoading(true);

        try {
            await adminApi.submitAccessRequest(parsed.data);
            setSubmitted(true);
        } catch (err: unknown) {
            if (axios.isAxiosError(err)) {
                const code = err.response?.data?.error as string | undefined;
                if (code === "already_pending") {
                    setErrors({ email: getTranslation("access_request.error_already_pending") });
                } else if (code === "already_approved") {
                    setErrors({ email: getTranslation("access_request.error_already_approved") });
                } else if (code === "too_many_pending") {
                    setErrors({ api: getTranslation("access_request.error_too_many_pending") });
                } else {
                    setErrors({ api: getTranslation("access_request.error_generic") });
                }
            } else {
                setErrors({ api: getTranslation("access_request.error_generic") });
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <Modal open={open} onClose={handleClose} className="max-w-md">
            {submitted ? (
                <div className="flex flex-col items-center gap-4 py-8 px-2 text-center">
                    <div className="h-16 w-16 rounded-full bg-primary/10 flex items-center justify-center">
                        <PartyPopper className="w-8 h-8 text-primary" />
                    </div>
                    <div className="space-y-1">
                        <p className="text-lg font-semibold">{getTranslation("access_request.success_title")}</p>
                        <p className="text-sm text-muted-foreground leading-relaxed">
                            {getTranslation("access_request.success_description")}
                        </p>
                    </div>
                    <Button className="mt-2" onClick={handleClose}>
                        {getTranslation("common.close")}
                    </Button>
                </div>
            ) : (
                <div className="space-y-5">
                    {/* Honeypot: hidden from users, filled only by bots */}
                    <div aria-hidden="true" className="absolute -left-[9999px] -top-[9999px] overflow-hidden">
                        <label htmlFor="req-website">Website</label>
                        <input
                            id="req-website"
                            type="text"
                            value={honeypot}
                            onChange={(e) => setHoneypot(e.target.value)}
                            tabIndex={-1}
                            autoComplete="off"
                        />
                    </div>

                    {/* Header */}
                    <div>
                        <h2 className="text-lg font-semibold">{getTranslation("access_request.modal_title")}</h2>
                        <p className="text-sm text-muted-foreground mt-1 leading-relaxed">
                            {getTranslation("access_request.modal_description")}
                        </p>
                    </div>

                    {/* Email */}
                    <div className="space-y-1">
                        <p className="text-sm font-medium">{getTranslation("access_request.email_label")}</p>
                        <Input
                            id="req-email"
                            type="email"
                            value={email}
                            onChange={(e) => { setEmail(e.target.value); setErrors({}); }}
                            placeholder={getTranslation("access_request.email_placeholder")}
                            aria-invalid={!!errors.email}
                            className={errors.email ? "border-red-500 focus-visible:ring-red-500" : ""}
                        />
                        <p className="text-xs text-muted-foreground">{getTranslation("access_request.email_hint")}</p>
                        <FieldError message={errors.email} />
                    </div>

                    {/* Message */}
                    <div className="space-y-1">
                        <p className="text-sm font-medium">{getTranslation("access_request.message_label")}</p>
                        <Textarea
                            id="req-message"
                            value={message}
                            onChange={(e) => setMessage(e.target.value)}
                            placeholder={getTranslation("access_request.message_placeholder")}
                            rows={3}
                        />
                    </div>

                    <FieldError message={errors.api} />

                    <div className="flex justify-end gap-2 pt-1">
                        <Button variant="ghost" onClick={handleClose} disabled={isLoading}>
                            {getTranslation("common.cancel")}
                        </Button>
                        <Button onClick={handleSubmit} disabled={isLoading}>
                            <Send className="w-4 h-4 mr-2" />
                            {isLoading
                                ? getTranslation("access_request.submitting")
                                : getTranslation("access_request.submit_button")}
                        </Button>
                    </div>
                </div>
            )}
        </Modal>
    );
};
