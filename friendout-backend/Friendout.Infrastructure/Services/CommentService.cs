using System;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Comment;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly FriendoutDbContext _dbContext;
    private readonly ILogger<CommentService> _logger;
    private readonly IActivitiesHubNotifier _hubNotifier;

    public CommentService(FriendoutDbContext dbContext, ILogger<CommentService> logger, IActivitiesHubNotifier hubNotifier)
    {
        _dbContext = dbContext;
        _logger = logger;
        _hubNotifier = hubNotifier;
    }

    public async Task<ServiceResult<CommentDto>> CreateCommentAsync(string activityId, string userId, string content)
    {
        if (string.IsNullOrWhiteSpace(activityId))
            return ServiceResult<CommentDto>.Failure("ActivityId is required.");

        if (string.IsNullOrWhiteSpace(userId))
            return ServiceResult<CommentDto>.Failure("UserId is required.");

        if (string.IsNullOrWhiteSpace(content))
            return ServiceResult<CommentDto>.Failure("Content cannot be empty.");

        try
        {
            var activityExists = await _dbContext.Activities
                .AsNoTracking()
                .AnyAsync(a => a.Id == activityId);

            if (!activityExists)
                return ServiceResult<CommentDto>.Failure("Activity not found.");

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return ServiceResult<CommentDto>.Failure("User not found.");

            var now = DateTime.UtcNow;

            var comment = new ActivityComment
            {
                Id = Guid.NewGuid().ToString(),
                ActivityId = activityId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            var dto = new CommentDto
            {
                CommentId = comment.Id,
                SendBy = user.Name,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId
            };

            _ = _hubNotifier.NotifyNewCommentAsync(activityId, dto);

            return ServiceResult<CommentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating comment for activity {ActivityId}", activityId);
            return ServiceResult<CommentDto>.Failure("An error occurred while creating the comment.");
        }
    }

    public async Task<ServiceResult<CommentDto>> UpdateCommentAsync(string activityId, string commentId, string userId, string content)
    {
        if (string.IsNullOrWhiteSpace(activityId))
            return ServiceResult<CommentDto>.Failure("ActivityId is required.");

        if (string.IsNullOrWhiteSpace(commentId))
            return ServiceResult<CommentDto>.Failure("CommentId is required.");

        if (string.IsNullOrWhiteSpace(userId))
            return ServiceResult<CommentDto>.Failure("UserId is required.");

        if (string.IsNullOrWhiteSpace(content))
            return ServiceResult<CommentDto>.Failure("Content cannot be empty.");

        try
        {
            var comment = await _dbContext.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.ActivityId == activityId);

            if (comment is null)
                return ServiceResult<CommentDto>.Failure("Comment not found.");

            if (!string.Equals(comment.UserId, userId, StringComparison.Ordinal))
                return ServiceResult<CommentDto>.Failure("You are not allowed to edit this comment.");

            comment.Content = content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            _dbContext.Comments.Update(comment);
            await _dbContext.SaveChangesAsync();

            var dto = new CommentDto
            {
                CommentId = comment.Id,
                SendBy = comment.User.Name,
                UserId = comment.UserId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };

            _ = _hubNotifier.NotifyCommentUpdatedAsync(activityId, dto);

            return ServiceResult<CommentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while updating comment {CommentId} for activity {ActivityId}",
                commentId,
                activityId);
            return ServiceResult<CommentDto>.Failure("An error occurred while updating the comment.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteCommentAsync(string activityId, string commentId, string userId)
    {
        if (string.IsNullOrWhiteSpace(activityId))
            return ServiceResult<bool>.Failure("ActivityId is required.");

        if (string.IsNullOrWhiteSpace(commentId))
            return ServiceResult<bool>.Failure("CommentId is required.");

        if (string.IsNullOrWhiteSpace(userId))
            return ServiceResult<bool>.Failure("UserId is required.");

        try
        {
            var comment = await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.ActivityId == activityId);

            if (comment is null)
                return ServiceResult<bool>.Failure("Comment not found.");

            if (!string.Equals(comment.UserId, userId, StringComparison.Ordinal))
                return ServiceResult<bool>.Failure("You are not allowed to delete this comment.");

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();

            _ = _hubNotifier.NotifyCommentDeletedAsync(activityId, commentId);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while deleting comment {CommentId} for activity {ActivityId}",
                commentId,
                activityId);
            return ServiceResult<bool>.Failure("An error occurred while deleting the comment.");
        }
    }
}

