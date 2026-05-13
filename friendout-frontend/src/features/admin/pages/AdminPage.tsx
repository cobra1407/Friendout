import { useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { authApi } from "@/features/auth/api/auth.api";
import { ActivityLayout } from "@/features/activity/layout/activityLayout";
import { Header } from "@/components/header";
import { UserRole } from "@/features/user/enum/userRole.enum";
import { AdminPageHeader } from "../components/AdminPageHeader";
import { AdminStatsSummary } from "../components/AdminStatsSummary";
import { AdminUsersSection } from "../components/AdminUsersSection";
import { AdminGuildsSection } from "../components/AdminGuildsSection";
import { AdminEmailsSection } from "../components/AdminEmailsSection";
import { AdminAccessRequestsModal } from "../components/AdminAccessRequestsModal";
import { AdminLogsSection } from "../components/AdminLogsSection";
import { LayoutDashboard, ScrollText } from "lucide-react";
import { getTranslation } from "@/i18n";

type AdminTab = "dashboard" | "logs";

const TABS: { id: AdminTab; labelKey: string; icon: React.ReactNode }[] = [
    { id: "dashboard", labelKey: "admin.nav.dashboard", icon: <LayoutDashboard className="w-4 h-4" /> },
    { id: "logs", labelKey: "admin.nav.logs", icon: <ScrollText className="w-4 h-4" /> },
];

export default function AdminPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const [requestsOpen, setRequestsOpen] = useState(false);

    const activeTab = (searchParams.get("tab") as AdminTab | null) ?? "dashboard";
    const setActiveTab = (tab: AdminTab) => setSearchParams({ tab }, { replace: true });

    if (!user || user.role !== UserRole.Admin) {
        if (user) navigate("/activities");
        return null;
    }

    const handleLogout = async () => {
        await authApi.logout();
        navigate("/login");
    };

    return (
        <ActivityLayout
            header={
                <Header
                    onCreateActivity={() => navigate("/activities/createActivity")}
                    onLogout={handleLogout}
                />
            }
        >
            <div className="max-w-7xl mx-auto w-full pb-10 space-y-6 px-4">
                <AdminPageHeader />

                {/* Navigation */}
                <nav className="flex gap-1 border-b">
                    {TABS.map(tab => (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`flex items-center gap-2 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors -mb-px cursor-pointer ${activeTab === tab.id
                                ? "border-primary text-primary"
                                : "border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground/40"
                                }`}
                        >
                            {tab.icon}
                            {getTranslation(tab.labelKey)}
                        </button>
                    ))}
                </nav>

                {/* Dashboard */}
                {activeTab === "dashboard" && (
                    <>
                        <AdminStatsSummary onOpenRequests={() => setRequestsOpen(true)} />
                        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                            <div className="lg:col-span-2">
                                <AdminUsersSection />
                            </div>
                            <div className="flex flex-col gap-6">
                                <AdminGuildsSection />
                                <AdminEmailsSection />
                            </div>
                        </div>
                    </>
                )}

                {/* Logs */}
                {activeTab === "logs" && <AdminLogsSection />}
            </div>

            <AdminAccessRequestsModal
                open={requestsOpen}
                onClose={() => setRequestsOpen(false)}
            />
        </ActivityLayout>
    );
}
