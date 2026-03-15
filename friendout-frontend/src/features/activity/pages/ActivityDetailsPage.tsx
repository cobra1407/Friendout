import { useNavigate, useParams } from "react-router-dom";
import ActivityHeader from "@/features/activity/components/ActivityHeader";
import { ActivityLayout } from "@/features/activity/layout/activityLayout";
import { Spinner } from "@/components/ui/spinner";
import { ErrorState } from "@/features/error/components/ErrorState";
import findIcon from "@/assets/images/find-icon.svg";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { useActivityDetails } from "@/features/activity/hooks/useActivityDetails";
import { useActivityEquipmentSync } from "@/features/activity/hooks/useActivityEquipmentSync";
import { useActivityParticipationSync } from "@/features/activity/hooks/useActivityParticipationSync";
import { useActivityCommentHandlers } from "@/features/activity/hooks/useActivityCommentHandlers";
import { ActivityDetailsContent } from "@/features/activity/components/ActivityDetailsContent";
import { getTranslation } from "@/i18n";
import api from "@/lib/api/api";

export const ActivityDetailsPage = () => {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const { user } = useAuth();

    const { activityDetails, setActivityDetails, isLoading } = useActivityDetails(id);
    const equipmentActions = useActivityEquipmentSync(
        activityDetails,
        setActivityDetails
    );
    const participationActions = useActivityParticipationSync(
        activityDetails,
        setActivityDetails
    );
    const commentHandlers = useActivityCommentHandlers({
        onCommentCreated: (newComment) => {
            setActivityDetails((prev) => {
                if (!prev) return prev;

                return {
                    ...prev,
                    comments: [newComment, ...(prev.comments ?? [])],
                };
            });
        },
        onCommentUpdated: (updatedComment) => {
            setActivityDetails((prev) => {
                if (!prev) return prev;

                return {
                    ...prev,
                    comments: prev.comments.map((comment) =>
                        comment.commentId === updatedComment.commentId
                            ? updatedComment
                            : comment
                    ),
                };
            });
        },
        onCommentDeleted: (deletedCommentId) => {
            setActivityDetails((prev) => {
                if (!prev) return prev;

                return {
                    ...prev,
                    comments: prev.comments.filter(
                        (comment) => comment.commentId !== deletedCommentId
                    ),
                };
            });
        },
    });

    const handleOnBack = () => navigate("/activities");

    const handleDeleteActivity = () => {
        api.delete(`/activities/${id}`)
            .then(() => {
                navigate("/activities");
            })
            .catch((err) => {
                if (import.meta.env.DEV) {
                    console.error("Erreur suppression activité:", err);
                }
                alert(getTranslation("activity.delete_error"));
            });
    };

    if (isLoading) {
        return (
            <ActivityLayout>
                <div className="flex items-center justify-center h-full">
                    <Spinner className="w-8 h-8" />
                </div>
            </ActivityLayout>
        );
    }

    if (!activityDetails) {
        return (
            <ActivityLayout header={<div className="h-16" />}>
                <ErrorState
                    title={getTranslation('activity.not_found')}
                    description={getTranslation('activity.not_found_description')}
                    icon={
                        <img src={findIcon} alt={getTranslation('activity.not_found_icon_alt')} className="h-8 w-8" />
                    }
                    primaryAction={{ label: getTranslation('common.back'), onClick: handleOnBack }}
                />
            </ActivityLayout>
        );
    }

    return (
        <ActivityLayout
            header={
                <ActivityHeader
                    activity={activityDetails}
                    currentUserId={user?.name}
                    onBack={handleOnBack}
                    onEdit={(activity) => navigate(`/activities/${activity.id}/edit`)}
                    onDelete={handleDeleteActivity}
                />
            }
        >
            <ActivityDetailsContent
                activity={activityDetails}
                currentUserId={user?.userId}
                onMainParticipationChange={
                    participationActions.handleMainParticipationChange
                }
                onSubActivitiesParticipationChange={
                    participationActions.handleSubActivitiesParticipationChange
                }
                getSubActivitySelectedStatus={
                    participationActions.getSubActivitySelectedStatus
                }
                getSubActivitiesSelectedStatus={
                    participationActions.getSubActivitiesSelectedStatus
                }
                onToggleEquipment={equipmentActions.handleToggleEquipment}
                commentsProps={{
                    ...commentHandlers,
                    onSubmit: () => {
                        if (!id) return;
                        commentHandlers.handleSubmitComment(id, commentHandlers.newComment);
                    },
                    handleUpdateComment: (commentId: string) => {
                        if (!id) return;
                        commentHandlers.handleUpdateComment(id, commentId);
                    },
                    handleDeleteComment: (commentId: string) => {
                        if (!id) return;
                        commentHandlers.handleDeleteComment(id, commentId);
                    },
                }}
            />
        </ActivityLayout>
    );
};

export default ActivityDetailsPage;
