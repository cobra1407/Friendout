import api from "@/lib/api/api";
import type { Comment } from "@/features/comment/types/comment.type";

interface CreateCommentParams {
    activityId: string;
    content: string;
}

export async function createComment(createCommentParams: CreateCommentParams) {
    const response = await api.post<Comment>(`/activities/${createCommentParams.activityId}/comments`, {
        content: createCommentParams.content,
    });
    return response.data;
}

interface UpdateCommentParams {
    activityId: string;
    commentId: string;
    content: string;
}

export async function updateComment(updateCommentParams: UpdateCommentParams) {
    const response = await api.put<Comment>(
        `/activities/${updateCommentParams.activityId}/comments/${updateCommentParams.commentId}`,
        {
            content: updateCommentParams.content,
        }
    );
    return response.data;
}

interface DeleteCommentParams {
    activityId: string;
    commentId: string;
}

export async function deleteComment(deleteCommentParams: DeleteCommentParams) {
    await api.delete(
        `/activities/${deleteCommentParams.activityId}/comments/${deleteCommentParams.commentId}`
    );
}
