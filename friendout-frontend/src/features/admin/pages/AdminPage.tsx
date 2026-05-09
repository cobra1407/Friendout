import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
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

export default function AdminPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [requestsOpen, setRequestsOpen] = useState(false);

    useEffect(() => {
        if (user && user.role !== UserRole.Admin) navigate("/activities");
    }, [user, navigate]);

    if (!user || user.role !== UserRole.Admin) return null;

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
            </div>

            <AdminAccessRequestsModal
                open={requestsOpen}
                onClose={() => setRequestsOpen(false)}
            />
        </ActivityLayout>
    );
}
