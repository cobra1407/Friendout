import { InfoIcon, Mail, Plus, Trash2, TriangleAlert } from "lucide-react";
import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal";
import { getTranslation } from "@/i18n";
import { useAdminEmails, useAccessMode } from "../hooks/useAdmin";

export const AdminEmailsSection = () => {
    const { emails, isLoading, email, setEmail, addMutation, deleteMutation } = useAdminEmails();
    const { accessMode } = useAccessMode();
    const [pendingDeleteId, setPendingDeleteId] = useState<number | null>(null);
    const pendingEmail = emails.find(e => e.id === pendingDeleteId);

    const handleAddKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter" && email.trim() && !addMutation.isPending) {
            addMutation.mutate();
        }
    };

    return (
        <>
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
                    {accessMode?.isGoogleOpenMode && (
                        <div className="flex items-start gap-2 rounded-lg border border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-950/40 px-3 py-2.5">
                            <TriangleAlert className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
                            <p className="text-xs text-amber-800 dark:text-amber-300">
                                {getTranslation('admin.emails.open_mode_warning')}
                            </p>
                        </div>
                    )}
                    {!accessMode?.isGoogleOpenMode && accessMode?.isGoogleRestrictionLocksEveryone && !accessMode?.noLoginMethodAvailable && (
                        <div className="flex items-start gap-2 rounded-lg border border-sky-200 bg-sky-50 dark:border-sky-800 dark:bg-sky-950/40 px-3 py-2.5">
                            <InfoIcon className="w-4 h-4 text-sky-600 dark:text-sky-400 shrink-0 mt-0.5" />
                            <p className="text-xs text-sky-800 dark:text-sky-300">
                                {getTranslation('admin.emails.disabled_as_login_method_info')}
                            </p>
                        </div>
                    )}
                    {accessMode?.noLoginMethodAvailable && (
                        <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-950/40 px-3 py-2.5">
                            <TriangleAlert className="w-4 h-4 text-red-600 dark:text-red-400 shrink-0 mt-0.5" />
                            <p className="text-xs text-red-800 dark:text-red-300">
                                {getTranslation('admin.emails.no_login_method_warning')}
                            </p>
                        </div>
                    )}
                    <div className="flex gap-2">
                        <Input
                            type="email"
                            placeholder="John@gmail.com"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            onKeyDown={handleAddKeyDown}
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
                                        onClick={() => setPendingDeleteId(e.id)}
                                    >
                                        <Trash2 className="w-3 h-3" />
                                    </Button>
                                </li>
                            ))}
                        </ul>
                    )}
                </CardContent>
            </Card>

            <Modal
                open={pendingDeleteId !== null}
                onClose={() => setPendingDeleteId(null)}
                className="max-w-sm"
            >
                <ModalHeader>
                    <div className="flex items-center gap-2">
                        <div className="p-2 rounded-full bg-red-50 dark:bg-red-950/40">
                            <Trash2 className="w-4 h-4 text-destructive" />
                        </div>
                        <ModalTitle>
                            {getTranslation('admin.emails.delete_confirm_title')}
                        </ModalTitle>
                    </div>
                    <ModalDescription>
                        {getTranslation('admin.emails.delete_confirm_description', {
                            email: pendingEmail?.email ?? ''
                        })}
                    </ModalDescription>
                </ModalHeader>
                <div className="flex justify-end gap-2 mt-4">
                    <Button
                        variant="outline"
                        onClick={() => setPendingDeleteId(null)}
                        disabled={deleteMutation.isPending}
                    >
                        {getTranslation('admin.users.cancel')}
                    </Button>
                    <Button
                        variant="destructive"
                        disabled={deleteMutation.isPending}
                        onClick={() => {
                            if (pendingDeleteId !== null) {
                                deleteMutation.mutate(pendingDeleteId, {
                                    onSettled: () => setPendingDeleteId(null)
                                });
                            }
                        }}
                    >
                        {deleteMutation.isPending
                            ? <Spinner className="w-4 h-4" />
                            : getTranslation('admin.emails.delete_confirm')
                        }
                    </Button>
                </div>
            </Modal>
        </>
    );
};
