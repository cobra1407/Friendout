using System.Security.Claims;
using friendout_backend.RequestModels.Comment;
using Friendout.Domain.DTOs.Comment;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller for managing comments on activities.
/// </summary>
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Creates a new comment for an activity.
    /// </summary>
    [Authorize]
    [HttpPost("activities/{activityId}/comments")]
    [ProducesResponseType(typeof(CommentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateComment(
        [FromRoute] string activityId,
        [FromBody] CreateCommentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        if (string.IsNullOrWhiteSpace(activityId))
            return BadRequest("ActivityId is required");

        var result = await _commentService.CreateCommentAsync(activityId, userId, request.Content);

        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Updates an existing comment for an activity.
    /// </summary>
    [Authorize]
    [HttpPut("activities/{activityId}/comments/{commentId}")]
    [ProducesResponseType(typeof(CommentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateComment(
        [FromRoute] string activityId,
        [FromRoute] string commentId,
        [FromBody] UpdateCommentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        if (string.IsNullOrWhiteSpace(activityId) || string.IsNullOrWhiteSpace(commentId))
            return BadRequest("ActivityId and CommentId are required");

        var result = await _commentService.UpdateCommentAsync(activityId, commentId, userId, request.Content);

        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Deletes an existing comment for an activity.
    /// </summary>
    [Authorize]
    [HttpDelete("activities/{activityId}/comments/{commentId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteComment(
        [FromRoute] string activityId,
        [FromRoute] string commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        if (string.IsNullOrWhiteSpace(activityId) || string.IsNullOrWhiteSpace(commentId))
            return BadRequest("ActivityId and CommentId are required");

        var result = await _commentService.DeleteCommentAsync(activityId, commentId, userId);

        if (result.IsSuccess)
            return NoContent();

        return BadRequest(result.ErrorMessage);
    }
}

