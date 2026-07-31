import { ActivityLayout } from "../layout/activityLayout"
import ActivityForm from "../components/ActivityForm";
import { useNavigate } from "react-router";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { getTranslation } from "@/i18n";

interface CreateActivityPageProps {
    mode?: "create" | "edit";
}

const CreateActivityPage = ({ mode = "create" }: CreateActivityPageProps) => {
    const navigate = useNavigate();

    const header = (
        <header className="bg-background shadow-sm border-b">
            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center h-16">
                    <Button
                        type="button"
                        variant="ghost"
                        onClick={() => navigate("/activities")}
                        className="flex items-center gap-2 mr-4"
                    >
                        <ArrowLeft className="w-4 h-4" />
                        {getTranslation("common.back")}
                    </Button>
                    <h1 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">
                        {mode === "create"
                            ? getTranslation("create_activity_page.create.title")
                            : getTranslation("create_activity_page.edit.title")}
                    </h1>
                </div>
            </div>
        </header>
    );

    return (
        <ActivityLayout header={header}>
            <ActivityForm mode="create" onBack={() => navigate("/activities")} onSuccess={() => { }} />
        </ActivityLayout>
    );
};

export default CreateActivityPage;
