import { useEffect } from "react";
import { getHubConnection, waitForHubConnection } from "@/lib/signalr/hubConnection";
import type { Comment } from "@/features/comment/types/comment.type";
import type { UserActivityParticipants } from "@/features/participant/types/UserActivityParticipants";

interface UseRealtimeActivityDetailOptions {
    activityId: string | undefined;
    onNewComment: (comment: Comment) => void;
    onCommentUpdated: (comment: Comment) => void;
    onCommentDeleted: (commentId: string) => void;
    onParticipantsChanged: (update: UserActivityParticipants) => void;
    onActivityDeleted: () => void;
}

/**
 * Joins the per-activity SignalR group while the detail page is mounted (leaves it on unmount
 * or when navigating to a different activity), and wires the comment/participant real-time
 * events into the same callbacks the page already uses for its optimistic REST-driven updates
 * (useActivityCommentHandlers, useActivityParticipationSync) — no separate state path needed,
 * a real-time event just triggers the same merge logic a successful API call would have.
 */
export function useRealtimeActivityDetail({
    activityId,
    onNewComment,
    onCommentUpdated,
    onCommentDeleted,
    onParticipantsChanged,
    onActivityDeleted,
}: UseRealtimeActivityDetailOptions) {
    useEffect(() => {
        if (!activityId) return;

        const connection = getHubConnection();
        let didLeave = false;

        const handleDeletedActivity = (deletedActivityId: string) => {
            if (deletedActivityId === activityId) onActivityDeleted();
        };

        connection.on("NewComment", onNewComment);
        connection.on("CommentUpdated", onCommentUpdated);
        connection.on("CommentDeleted", onCommentDeleted);
        connection.on("ParticipantsChanged", onParticipantsChanged);
        connection.on("DeletedActivity", handleDeletedActivity);

        waitForHubConnection().then(() => {
            if (didLeave) return;
            connection.invoke("JoinActivityGroup", activityId).catch(() => {
            });
        });

        return () => {
            didLeave = true;
            connection.off("NewComment", onNewComment);
            connection.off("CommentUpdated", onCommentUpdated);
            connection.off("CommentDeleted", onCommentDeleted);
            connection.off("ParticipantsChanged", onParticipantsChanged);
            connection.off("DeletedActivity", handleDeletedActivity);
            connection.invoke("LeaveActivityGroup", activityId).catch(() => {
            });
        };
    }, [activityId, onNewComment, onCommentUpdated, onCommentDeleted, onParticipantsChanged, onActivityDeleted]);
}
