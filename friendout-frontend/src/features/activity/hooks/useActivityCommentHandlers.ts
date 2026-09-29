import { useState, useCallback } from "react";
import { useMutation } from "@tanstack/react-query";
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
    const [editingCommentId, setEditingCommentId] = useState<string | undefined>();
    const [editedCommentContent, setEditedCommentContent] = useState("");

    const createMutation = useMutation({
        mutationFn: (vars: { activityId: string; content: string }) => createComment(vars),
        onSuccess: (createdComment) => {
            onCommentCreated?.(createdComment);
            setNewComment("");
        },
        onError: (error) => console.error("Error creating comment", { error }),
    });

    const updateMutation = useMutation({
        mutationFn: (vars: { activityId: string; commentId: string; content: string }) => updateComment(vars),
        onSuccess: (updatedComment) => {
            onCommentUpdated?.(updatedComment);
            setEditingCommentId(undefined);
            setEditedCommentContent("");
        },
        onError: (error) => console.error("Error updating comment", { error }),
    });

    const deleteMutation = useMutation({
        mutationFn: (vars: { activityId: string; commentId: string }) => deleteComment(vars),
        onSuccess: (_data, vars) => {
            onCommentDeleted?.(vars.commentId);
            setEditingCommentId((current) => (current === vars.commentId ? undefined : current));
        },
        onError: (error) => console.error("Error deleting comment", { error }),
    });

    const handleEditComment = useCallback(
        (comment: ActivityDetails["comments"][number]) => {
            setEditingCommentId(comment.commentId);
            setEditedCommentContent(comment.content);
        },
        []
    );

    const handleUpdateComment = useCallback(
        (activityId: string, commentId: string) => {
            if (editingCommentId && editingCommentId !== commentId) return;

            const trimmedContent = editedCommentContent.trim();
            if (!trimmedContent) return;

            updateMutation.mutate({ activityId, commentId, content: trimmedContent });
        },
        [editedCommentContent, editingCommentId, updateMutation]
    );

    const handleDeleteComment = useCallback(
        (activityId: string, commentId: string) => {
            deleteMutation.mutate({ activityId, commentId });
        },
        [deleteMutation]
    );

    const handleSubmitComment = useCallback((activityId: string, content: string) => {
        const trimmedContent = content.trim();
        if (!trimmedContent) return;

        createMutation.mutate({ activityId, content: trimmedContent });
    }, [createMutation]);

    const cancelEdit = useCallback(() => {
        setEditingCommentId(undefined);
        setEditedCommentContent("");
    }, []);

    return {
        newComment,
        setNewComment,
        isSubmittingComment: createMutation.isPending,
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
