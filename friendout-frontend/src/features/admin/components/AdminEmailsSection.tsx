import { Mail, Plus, Trash2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { getTranslation } from "@/i18n";
import { useAdminEmails } from "../hooks/useAdmin";

export const AdminEmailsSection = () => {
    const { emails, isLoading, email, setEmail, addMutation, deleteMutation } = useAdminEmails();

    return (
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
                <div className="flex gap-2">
                    <Input
                        type="email"
                        placeholder="prenom@gmail.com"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
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
                                    onClick={() => deleteMutation.mutate(e.id)}
                                >
                                    <Trash2 className="w-3 h-3" />
                                </Button>
                            </li>
                        ))}
                    </ul>
                )}
            </CardContent>
        </Card>
    );
};
