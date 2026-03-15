import * as React from "react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { MessageCircle, Check, X } from "lucide-react";

import type { Comment } from "@/features/comment/types/comment.type";
import CommentActions from "./CommentAction";
import { getTranslation } from "@/i18n";

interface CommentsSectionProps {
    comments: Comment[];
    currentUserId?: string;
    newComment: string;
    setNewComment: (val: string) => void;
    isSubmittingComment: boolean;
    onSubmit: () => void;
    editingCommentId?: string;
    editedCommentContent: string;
    setEditedCommentContent: (val: string) => void;
    handleEditComment: (comment: Comment) => void;
    handleUpdateComment: (commentId: string) => void;
    cancelEdit: () => void;
    handleDeleteComment: (commentId: string) => void;
    formatCommentDate: (date: string) => string;
}

export default function CommentsSection({
    comments,
    currentUserId,
    newComment,
    setNewComment,
    isSubmittingComment,
    onSubmit,
    editingCommentId,
    editedCommentContent,
    setEditedCommentContent,
    handleEditComment,
    handleUpdateComment,
    cancelEdit,
    handleDeleteComment,
    formatCommentDate,
}: CommentsSectionProps) {
    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <MessageCircle className="w-5 h-5" />
                    {getTranslation('comments.title_count', { count: comments.length })}
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
                {currentUserId && (
                    <form
                        onSubmit={(e) => {
                            e.preventDefault();
                            onSubmit();
                        }}
                        className="space-y-3"
                    >
                        <Textarea
                            value={newComment}
                            onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setNewComment(e.target.value)}
                            onKeyDown={(e: React.KeyboardEvent<HTMLTextAreaElement>) => {
                                if (e.key === "Enter" && !e.shiftKey) {
                                    e.preventDefault();
                                    onSubmit();
                                }
                            }}
                            placeholder={getTranslation('comments.placeholder')}
                            rows={3}
                        />
                        <Button
                            type="submit"
                            disabled={!newComment.trim() || isSubmittingComment}
                            size="sm"
                        >
                            {isSubmittingComment ? getTranslation('comments.sending') : getTranslation('comments.submit')}
                        </Button>
                    </form>
                )}

                {comments.length === 0 ? (
                    <p className="text-gray-500 text-center py-4">
                        {getTranslation('comments.empty')}
                    </p>
                ) : (
                    <div className="space-y-4">
                        {comments.map((comment) => (
                            <div key={comment.commentId} className="bg-gray-50 rounded-lg p-4">
                                <div className="flex justify-between items-start mb-2">
                                    <div>
                                        <span className="font-medium">{comment.sendBy}</span>
                                        <span className="text-sm text-gray-500 ml-2">
                                            {formatCommentDate(comment.createdAt)}
                                            {comment.updatedAt !== comment.createdAt && ` ${getTranslation('comments.modified')}`}
                                        </span>
                                    </div>
                                    {currentUserId === comment.userId && (
                                        <div className="flex items-center gap-0.5">
                                            {editingCommentId === comment.commentId ? (
                                                <>
                                                    <button
                                                        type="button"
                                                        onClick={() => handleUpdateComment(comment.commentId)}
                                                        className="p-1.5 text-gray-500 hover:bg-gray-100 rounded-full transition-colors"
                                                        title={getTranslation('common.save')}
                                                    >
                                                        <Check className="h-3.5 w-3.5" />
                                                    </button>
                                                    <button
                                                        type="button"
                                                        onClick={cancelEdit}
                                                        className="p-1.5 text-gray-500 hover:bg-gray-100 rounded-full transition-colors"
                                                        title={getTranslation('common.cancel')}
                                                    >
                                                        <X className="h-3.5 w-3.5" />
                                                    </button>
                                                </>
                                            ) : (
                                                <CommentActions
                                                    onEdit={() => handleEditComment(comment)}
                                                    onDelete={() => handleDeleteComment(comment.commentId)}
                                                />
                                            )}
                                        </div>
                                    )}
                                </div>

                                {editingCommentId === comment.commentId ? (
                                    <Textarea
                                        value={editedCommentContent}
                                        onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setEditedCommentContent(e.target.value)}
                                        className="mt-2"
                                        autoFocus
                                    />
                                ) : (
                                    <p className="text-gray-700 whitespace-pre-wrap">
                                        {comment.content}
                                    </p>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
