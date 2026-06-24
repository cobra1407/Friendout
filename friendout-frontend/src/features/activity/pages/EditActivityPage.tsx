import { ActivityLayout } from "../layout/activityLayout";
import ActivityForm from "../components/ActivityForm";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { ErrorState } from "@/features/error/components/ErrorState";
import findIcon from "@/assets/images/find-icon.svg";
import { getTranslation } from "@/i18n";
import { useActivityDetails } from "@/features/activity/hooks/useActivityDetails";

const EditActivityPage = () => {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const { activityDetails, isLoading } = useActivityDetails(id);

    const header = (
        <header className="bg-background shadow-sm border-b">
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center h-16">
                    <Button type="button" variant="ghost" onClick={() => navigate(id ? `/activities/${id}` : "/activities")} className="flex items-center gap-2 mr-4">
                        <ArrowLeft className="w-4 h-4" />
                        {getTranslation("common.back")}
                    </Button>
                    <h1 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
                        {getTranslation("create_activity_page.edit.title")}
                    </h1>
                </div>
            </div>
        </header>
    );

    if (isLoading) {
        return (
            <ActivityLayout header={header}>
                <div className="flex items-center justify-center h-full">
                    <Spinner className="w-8 h-8" />
                </div>
            </ActivityLayout>
        );
    }

    if (!activityDetails) {
        return (
            <ActivityLayout header={header}>
                <ErrorState
                    title={getTranslation("activity.not_found")}
                    description={getTranslation("activity.not_found_description")}
                    icon={<img src={findIcon} alt={getTranslation("activity.not_found_icon_alt")} className="h-8 w-8" />}
                    primaryAction={{ label: getTranslation("common.back"), onClick: () => navigate("/activities") }}
                />
            </ActivityLayout>
        );
    }

    return (
        <ActivityLayout header={header}>
            <ActivityForm
                mode="edit"
                initialData={activityDetails}
                onBack={() => navigate(`/activities/${activityDetails.id}`)}
                onSuccess={() => { }}
            />
        </ActivityLayout>
    );
};

export default EditActivityPage;
