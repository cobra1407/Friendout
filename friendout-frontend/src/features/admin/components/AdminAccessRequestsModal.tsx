import { AlertCircle, Check, User2Icon, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Modal, ModalHeader, ModalTitle, ModalDescription } from "@/components/ui/modal";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { useAdminAccessRequests } from "../hooks/useAdmin";

interface AdminAccessRequestsModalProps {
    open: boolean;
    onClose: () => void;
}

export const AdminAccessRequestsModal = ({ open, onClose }: AdminAccessRequestsModalProps) => {
    const { requests, isLoading, resolveMutation } = useAdminAccessRequests();

    return (
        <Modal open={open} onClose={onClose} className="max-w-lg">
            <ModalHeader>
                <ModalTitle className="flex items-center gap-2">
                    <AlertCircle className="w-4 h-4 text-amber-600" />
                    {getTranslation('admin.requests.modal_title')}
                    {requests.length > 0 && (
                        <Badge className="bg-amber-100 text-amber-700 border-amber-200 ml-1">
                            {requests.length}
                        </Badge>
                    )}
                </ModalTitle>
                <ModalDescription>
                    {getTranslation('admin.requests.modal_description')}
                </ModalDescription>
            </ModalHeader>

            <div className="mt-2 w-full">
                {isLoading ? (
                    <div className="flex justify-center py-8"><Spinner /></div>
                ) : requests.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-10 text-muted-foreground">
                        <Check className="w-10 h-10 mb-2 opacity-20" />
                        <p className="text-sm italic">{getTranslation('admin.requests.empty')}</p>
                    </div>
                ) : (
                    <ul className="divide-y max-h-[60vh] overflow-y-auto w-full pr-1">
                        {requests.map((r) => (
                            <li key={r.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 py-3 first:pt-0 w-full min-w-0">
                                <div className="flex items-center gap-3 flex-1 min-w-0">
                                    <div className="h-8 w-8 rounded-full bg-muted-foreground/10 flex items-center justify-center shrink-0">
                                        <User2Icon className="w-5 h-5 text-muted-foreground shrink-0" />
                                    </div>

                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm font-medium truncate">{r.email}</p>
                                        {r.message && (
                                            /* break-all safely splits long continuous strings like code logs or unspaced URLs */
                                            <p className="text-xs text-muted-foreground italic mt-0.5 break-all whitespace-pre-line block w-full">
                                                "{r.message}"
                                            </p>
                                        )}
                                    </div>
                                </div>
                                {/* Action buttons */}
                                <div className="flex items-center gap-2 shrink-0 self-end sm:self-center">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="h-8 text-xs text-destructive border-destructive/20 hover:bg-destructive/5"
                                        disabled={resolveMutation.isPending}
                                        onClick={() => resolveMutation.mutate({ id: r.id, status: "Denied" })}
                                    >
                                        <X className="w-3.5 h-3.5 mr-1" /> {getTranslation('admin.requests.deny')}
                                    </Button>
                                    <Button
                                        size="sm"
                                        className="h-8 text-xs"
                                        disabled={resolveMutation.isPending}
                                        onClick={() => resolveMutation.mutate({ id: r.id, status: "Approved" })}
                                    >
                                        <Check className="w-3.5 h-3.5 mr-1" /> {getTranslation('admin.requests.approve')}
                                    </Button>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </Modal>
    );
};
