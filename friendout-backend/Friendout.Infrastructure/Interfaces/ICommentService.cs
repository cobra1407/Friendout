using System.Threading.Tasks;
using Friendout.Domain.DTOs.Comment;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface ICommentService
{
    /// <summary>
    /// Creates a new comment for a given activity.
    /// </summary>
    /// <param name="activityId">The activity identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="content">The comment content.</param>
    /// <returns>The created comment.</returns>
    Task<ServiceResult<CommentDto>> CreateCommentAsync(string activityId, string userId, string content);

    /// <summary>
    /// Updates an existing comment content.
    /// </summary>
    /// <param name="activityId">The activity identifier.</param>
    /// <param name="commentId">The comment identifier.</param>
    /// <param name="userId">The user identifier (must be the author).</param>
    /// <param name="content">The new content.</param>
    /// <returns>The updated comment.</returns>
    Task<ServiceResult<CommentDto>> UpdateCommentAsync(string activityId, string commentId, string userId, string content);

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    /// <param name="activityId">The activity identifier.</param>
    /// <param name="commentId">The comment identifier.</param>
    /// <param name="userId">The user identifier (must be the author).</param>
    /// <returns>True if deleted.</returns>
    Task<ServiceResult<bool>> DeleteCommentAsync(string activityId, string commentId, string userId);
}

