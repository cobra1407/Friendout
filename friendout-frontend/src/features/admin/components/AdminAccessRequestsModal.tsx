import { AlertCircle, Check, X } from "lucide-react";
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

            <div className="mt-2">
                {isLoading ? (
                    <div className="flex justify-center py-8"><Spinner /></div>
                ) : requests.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-10 text-muted-foreground">
                        <Check className="w-10 h-10 mb-2 opacity-20" />
                        <p className="text-sm italic">{getTranslation('admin.requests.empty')}</p>
                    </div>
                ) : (
                    <ul className="divide-y max-h-[60vh] overflow-y-auto">
                        {requests.map((r) => (
                            <li key={r.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 py-3 first:pt-0">
                                <div className="flex items-center gap-3">
                                    <div className="h-9 w-9 rounded-full bg-muted flex items-center justify-center text-sm font-semibold shrink-0">
                                        {r.name?.[0]?.toUpperCase() ?? "?"}
                                    </div>
                                    <div>
                                        <p className="text-sm font-medium">{r.name ?? getTranslation('admin.requests.unknown_name')}</p>
                                        <p className="text-xs text-muted-foreground">{r.email}</p>
                                        {r.message && (
                                            <p className="text-xs text-muted-foreground italic mt-0.5">"{r.message}"</p>
                                        )}
                                    </div>
                                </div>
                                <div className="flex items-center gap-2 shrink-0">
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
