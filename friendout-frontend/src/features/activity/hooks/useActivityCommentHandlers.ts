import { useState, useCallback } from "react";
import type { ActivityDetails } from "@/features/activity/types/activityDetails.type";
import {
    createComment,
    updateComment,
    deleteComment,
} from "@/features/comment/api/comment.api";

interface UseActivityCommentHandlersParams {
    onCommentCreated?: (newComment: ActivityDetails["comments"][number]) => void;
    onCommentUpdated?: (updatedComment: ActivityDetails["comments"][number]) => void;
    onCommentDeleted?: (commentId: string) => void;
}

export function useActivityCommentHandlers({
    onCommentCreated,
    onCommentUpdated,
    onCommentDeleted,
}: UseActivityCommentHandlersParams = {}) {
    const [newComment, setNewComment] = useState("");
    const [isSubmittingComment, setIsSubmittingComment] = useState(false);
    const [editingCommentId, setEditingCommentId] = useState<string | undefined>();
    const [editedCommentContent, setEditedCommentContent] = useState("");

    const handleEditComment = useCallback(
        (comment: ActivityDetails["comments"][number]) => {
            setEditingCommentId(comment.commentId);
            setEditedCommentContent(comment.content);
        },
        []
    );

    const handleUpdateComment = useCallback(
        async (activityId: string, commentId: string) => {
            if (editingCommentId && editingCommentId !== commentId) return;

            const trimmedContent = editedCommentContent.trim();
            if (!trimmedContent) return;

            try {
                const updatedComment = await updateComment({
                    activityId,
                    commentId,
                    content: trimmedContent,
                });
                onCommentUpdated?.(updatedComment);
                setEditingCommentId(undefined);
                setEditedCommentContent("");
            } catch (error) {
                console.error("Error updating comment", { error });
            }
        },
        [editedCommentContent, editingCommentId, onCommentUpdated]
    );

    const handleDeleteComment = useCallback(
        async (activityId: string, commentId: string) => {
            try {
                await deleteComment({ activityId, commentId });
                onCommentDeleted?.(commentId);
                if (editingCommentId === commentId) {
                    setEditingCommentId(undefined);
                    setEditedCommentContent("");
                }
            } catch (error) {
                console.error("Error deleting comment", { error });
            }
        },
        [editingCommentId, onCommentDeleted]
    );

    const handleSubmitComment = useCallback(async (activityId: string, content: string) => {
        const trimmedContent = content.trim();
        if (!trimmedContent) return;

        try {
            setIsSubmittingComment(true);
            const createdComment = await createComment({ activityId, content: trimmedContent });
            onCommentCreated?.(createdComment);
            setNewComment("");
        } catch (error) {
            console.error("Error creating comment", { error });
        } finally {
            setIsSubmittingComment(false);
        }
    }, [onCommentCreated]);

    const cancelEdit = useCallback(() => {
        setEditingCommentId(undefined);
        setEditedCommentContent("");
    }, []);

    return {
        newComment,
        setNewComment,
        isSubmittingComment,
        editingCommentId,
        editedCommentContent,
        setEditedCommentContent,
        handleEditComment,
        handleUpdateComment,
        handleDeleteComment,
        handleSubmitComment,
        cancelEdit,
    };
}
