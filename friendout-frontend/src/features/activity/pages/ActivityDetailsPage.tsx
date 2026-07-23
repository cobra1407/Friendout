import { useNavigate, useParams } from "react-router-dom";
import { useCallback, useState } from "react";
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
import { useRealtimeActivityDetail } from "@/features/realtime/hooks/useRealtimeActivityDetail";
import { ActivityDetailsContent } from "@/features/activity/components/ActivityDetailsContent";
import { getTranslation } from "@/i18n";
import { useOgMeta } from "@/lib/utils/useOgMeta";
import api from "@/lib/api/api";
import type { Comment } from "@/features/comment/types/comment.type";

export const ActivityDetailsPage = () => {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const { user } = useAuth();

    const { activityDetails, setActivityDetails, isLoading } = useActivityDetails(id);
    // clearer message can be shown instead of the generic not-found screen.
    const [wasDeletedWhileViewing, setWasDeletedWhileViewing] = useState(false);
    const equipmentActions = useActivityEquipmentSync(
        activityDetails,
        setActivityDetails
    );
    const participationActions = useActivityParticipationSync(
        activityDetails,
        setActivityDetails
    );

    // Lifted out of the useActivityCommentHandlers call below so the exact same function
    // references can also be passed to useRealtimeActivityDetail — a comment created by someone
    // else arrives as the same shape over the WebSocket, so it reuses the same merge logic
    // instead of duplicating it.
    const handleCommentCreated = useCallback((newComment: Comment) => {
        setActivityDetails((prev) => {
            if (!prev) return prev;

            const currentComments = prev.comments ?? [];

            // avoid duplicating a comment
            const commentExists = currentComments.some(
                (comment) => comment.commentId === newComment.commentId
            );

            if (commentExists) {
                return prev;
            }
            return {
                ...prev,
                comments: [newComment, ...currentComments]
            };
        });
    }, [setActivityDetails]);

    const handleCommentUpdated = useCallback((updatedComment: Comment) => {
        setActivityDetails((prev) => {
            if (!prev) return prev;
            return {
                ...prev,
                comments: prev.comments.map((comment) =>
                    comment.commentId === updatedComment.commentId ? updatedComment : comment
                ),
            };
        });
    }, [setActivityDetails]);

    const handleCommentDeleted = useCallback((deletedCommentId: string) => {
        setActivityDetails((prev) => {
            if (!prev) return prev;
            return {
                ...prev,
                comments: prev.comments.filter((comment) => comment.commentId !== deletedCommentId),
            };
        });
    }, [setActivityDetails]);

    const commentHandlers = useActivityCommentHandlers({
        onCommentCreated: handleCommentCreated,
        onCommentUpdated: handleCommentUpdated,
        onCommentDeleted: handleCommentDeleted,
    });

    useRealtimeActivityDetail({
        activityId: id,
        onNewComment: handleCommentCreated,
        onCommentUpdated: handleCommentUpdated,
        onCommentDeleted: handleCommentDeleted,
        onParticipantsChanged: participationActions.applyRealtimeParticipantsUpdate,
        // Shows a dedicated "this activity was just deleted" screen (see render below) rather
        // than the generic not-found one — someone mid-read gets a clear explanation of what just
        // happened instead of a message that sounds like they followed a broken/expired link.
        onActivityDeleted: () => setWasDeletedWhileViewing(true),
    });

    const handleOnBack = () => navigate("/activities");

    // Update Open Graph meta tags so WhatsApp / Telegram show the activity
    // title and image when the URL is shared.
    useOgMeta({
        title: activityDetails?.title ?? 'Friendout',
        description: activityDetails?.description,
        imageUrl: activityDetails?.image?.url ?? undefined,
        url: window.location.href,
    });

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

    if (wasDeletedWhileViewing) {
        return (
            <ActivityLayout header={<div className="h-16" />}>
                <ErrorState
                    title={getTranslation('activity.deleted_while_viewing')}
                    description={getTranslation('activity.deleted_while_viewing_description')}
                    icon={
                        <img src={findIcon} alt={getTranslation('activity.deleted_while_viewing_icon_alt')} className="h-8 w-8" />
                    }
                    primaryAction={{ label: getTranslation('common.back'), onClick: handleOnBack }}
                />
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
