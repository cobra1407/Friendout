import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { MessageCircle, Check, X, Edit, Trash2 } from "lucide-react";
import { formatDate } from "@/lib/utils/date.utils"; // à vérifier
import type { User } from "@/features/user/types/user.type";
import type { Comment } from "@/features/comment/types/comment.type";

type Props = {
    comments: Comment[];
    currentUser: User | null;
    newComment: string;
    setNewComment: (s: string) => void;
    editingCommentId: string | null;
    editedCommentContent: string;
    setEditedCommentContent: (s: string) => void;
    onSubmit: () => void;
    onEdit: (id: string, content: string) => void; // démarre l'édition
    onCancelEdit: () => void;
    onUpdate: () => void; // sauvegarde l'édition
    onDelete: (id: string) => void;
};

export default function ActivityComments({
    comments,
    currentUser,
    newComment,
    setNewComment,
    editingCommentId,
    editedCommentContent,
    setEditedCommentContent,
    onSubmit,
    onEdit,
    onCancelEdit,
    onUpdate,
    onDelete,
}: Props) {
    return (
        <Card>
            <CardHeader>
                <CardTitle className="flex items-center gap-2">
                    <MessageCircle className="w-5 h-5" />
                    Commentaires ({comments.length})
                </CardTitle>
            </CardHeader>

            <CardContent className="space-y-4">
                {/* Formulaire d'ajout */}
                {currentUser && (
                    <form
                        onSubmit={(e) => {
                            e.preventDefault();
                            if (newComment.trim()) onSubmit();
                        }}
                        className="space-y-3"
                    >
                        <Textarea
                            value={newComment}
                            onChange={(e) => setNewComment(e.target.value)}
                            onKeyDown={(e) => {
                                if (e.key === "Enter" && !e.shiftKey) {
                                    e.preventDefault();
                                    if (newComment.trim()) onSubmit();
                                }
                            }}
                            placeholder="Ajouter un commentaire..."
                            rows={3}
                        />
                        <div className="flex justify-end">
                            <Button type="submit" size="sm" disabled={!newComment.trim()}>
                                Commenter
                            </Button>
                        </div>
                    </form>
                )}

                {/* Liste des commentaires */}
                {comments.length === 0 ? (
                    <p className="text-gray-500 text-center py-4">Aucun commentaire pour le moment</p>
                ) : (
                    <div className="space-y-4">
                        {comments.map((comment) => (
                            <div key={comment.commentId} className="bg-gray-50 rounded-lg p-4">
                                <div className="flex justify-between items-start mb-2">
                                    <div>
                                        <span className="font-medium">{comment.sendBy || "Utilisateur inconnu"}</span>
                                        <span className="text-sm text-gray-500 ml-2">
                                            {formatDate(comment.createdAt)}
                                            {comment.updatedAt !== comment.createdAt && " (modifié)"}
                                        </span>
                                    </div>

                                    {/* Actions (modifier / supprimer) pour l'auteur */}
                                    {currentUser?.userId === comment.sendBy && (
                                        <div className="flex items-center gap-0.5">
                                            {editingCommentId === comment.commentId ? (
                                                <>
                                                    <button
                                                        type="button"
                                                        onClick={onUpdate}
                                                        className="p-1.5 text-gray-500 hover:bg-gray-100 rounded-full transition-colors"
                                                        title="Enregistrer"
                                                    >
                                                        <Check className="h-3.5 w-3.5" />
                                                    </button>

                                                    <button
                                                        type="button"
                                                        onClick={onCancelEdit}
                                                        className="p-1.5 text-gray-500 hover:bg-gray-100 rounded-full transition-colors"
                                                        title="Annuler"
                                                    >
                                                        <X className="h-3.5 w-3.5" />
                                                    </button>
                                                </>
                                            ) : (
                                                <>
                                                    <button
                                                        type="button"
                                                        onClick={() => onEdit(comment.commentId, comment.content)}
                                                        className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-full transition-colors"
                                                        title="Modifier le commentaire"
                                                    >
                                                        <Edit className="h-3.5 w-3.5" />
                                                    </button>

                                                    <button
                                                        type="button"
                                                        onClick={() => {
                                                            if (confirm("Êtes-vous sûr de vouloir supprimer ce commentaire ?")) {
                                                                onDelete(comment.commentId);
                                                            }
                                                        }}
                                                        className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-full transition-colors"
                                                        title="Supprimer le commentaire"
                                                    >
                                                        <Trash2 className="h-3.5 w-3.5" />
                                                    </button>
                                                </>
                                            )}
                                        </div>
                                    )}
                                </div>

                                {/* Contenu ou zone d'édition */}
                                {editingCommentId === comment.commentId ? (
                                    <Textarea
                                        value={editedCommentContent}
                                        onChange={(e) => setEditedCommentContent(e.target.value)}
                                        className="mt-2"
                                        autoFocus
                                        rows={4}
                                        onKeyDown={(e) => {
                                            if (e.key === "Enter" && !e.shiftKey) {
                                                e.preventDefault();
                                                onUpdate();
                                            }
                                        }}
                                    />
                                ) : (
                                    <p className="text-gray-700 whitespace-pre-wrap">{comment.content}</p>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
