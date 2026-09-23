import { useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import { ShieldAlert, CheckCircle2, XCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { accountDeletionApi } from "@/features/auth/api/accountDeletion.api";

type Status = "idle" | "loading" | "success" | "error";

export const AccountDeletionConfirmPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const token = searchParams.get("token");

    const [status, setStatus] = useState<Status>("idle");

    const handleConfirm = async () => {
        if (!token) return;
        setStatus("loading");
        try {
            await accountDeletionApi.confirmDeletion(token);
            setStatus("success");
        } catch {
            // Invalid or expired token is the only failure mode from this endpoint;
            // no need to branch on the specific error code here.
            setStatus("error");
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center px-4">
            <Card className="max-w-md w-full">
                <CardContent className="flex flex-col items-center gap-4 py-8 px-2 text-center">
                    {!token || status === "error" ? (
                        <>
                            <div className="h-16 w-16 rounded-full bg-destructive/10 flex items-center justify-center">
                                <XCircle className="w-8 h-8 text-destructive" />
                            </div>
                            <div className="space-y-1">
                                <p className="text-lg font-semibold">{getTranslation("account_deletion.confirm_error_title")}</p>
                                <p className="text-sm text-muted-foreground leading-relaxed">
                                    {getTranslation("account_deletion.confirm_error_description")}
                                </p>
                            </div>
                            <Button className="mt-2" onClick={() => navigate("/login")}>
                                {getTranslation("account_deletion.back_to_login")}
                            </Button>
                        </>
                    ) : status === "success" ? (
                        <>
                            <div className="h-16 w-16 rounded-full bg-primary/10 flex items-center justify-center">
                                <CheckCircle2 className="w-8 h-8 text-primary" />
                            </div>
                            <div className="space-y-1">
                                <p className="text-lg font-semibold">{getTranslation("account_deletion.confirm_success_title")}</p>
                                <p className="text-sm text-muted-foreground leading-relaxed">
                                    {getTranslation("account_deletion.confirm_success_description")}
                                </p>
                            </div>
                            <Button className="mt-2" onClick={() => navigate("/login")}>
                                {getTranslation("account_deletion.back_to_login")}
                            </Button>
                        </>
                    ) : (
                        <>
                            <div className="h-16 w-16 rounded-full bg-muted flex items-center justify-center">
                                <ShieldAlert className="w-8 h-8 text-muted-foreground" />
                            </div>
                            <div className="space-y-1">
                                <p className="text-lg font-semibold">{getTranslation("account_deletion.confirm_title")}</p>
                                <p className="text-sm text-muted-foreground leading-relaxed">
                                    {getTranslation("account_deletion.confirm_description")}
                                </p>
                            </div>
                            <Button
                                variant="destructive"
                                className="mt-2 w-full"
                                onClick={handleConfirm}
                                disabled={status === "loading"}
                            >
                                {status === "loading" ? (
                                    <Spinner className="size-4" />
                                ) : (
                                    getTranslation("account_deletion.confirm_button")
                                )}
                            </Button>
                        </>
                    )}
                </CardContent>
            </Card>
        </div>
    );
};
