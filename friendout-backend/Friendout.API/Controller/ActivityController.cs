using System.Security.Claims;
using friendout_backend.Mappers;
using friendout_backend.RequestModels.Activity;
using friendout_backend.RequestModels.Filters;
using Friendout.Domain.DTOs.Activity;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace friendout_backend.Controller;

/// <summary>
/// Activity controller
/// </summary>
[ApiController]
public class ActivityController : ControllerBase
{
    /// <summary>
    /// Activity service
    /// </summary>
    public readonly IActivityService _activityService;
    private readonly ILogger<ActivityController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityController"/> class.
    /// </summary>
    /// <param name="activityService"></param>
    /// <param name="logger"></param>
    public ActivityController(IActivityService activityService, ILogger<ActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of activities with optional filters and scroll pagination
    /// </summary>
    [Authorize]
    [HttpGet("activities")]
    [ProducesResponseType(typeof(List<ActivityDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetActivities([FromQuery] ActivityFilter filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var filterDto = new ActivityFilterDto
        {
            TimeFilter = filter.TimeFilter,
            OnlyOwnActivity = filter.OnlyOwnActivity,
            Search = filter.Search,
            Skip = filter.Skip,
            Take = filter.Take
        };

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var result = await _activityService.GetActivitiesAsync(userId, filterDto);
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Get activity by id
    /// </summary>
    [Authorize]
    [ProducesResponseType(typeof(ActivityDto), 200)]
    [ProducesResponseType(401)]
    [HttpGet("activities/{activityId}/details")]
    public async Task<IActionResult> GetActivityById([FromRoute] string activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var result = await _activityService.GetActivityByIdAsync(activityId, userId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.ErrorMessage);
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    [Authorize]
    [HttpPost("activities")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ActivityDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateActivity([FromForm] CreateActivityRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        try
        {
            var rawEquipmentCount = request.RequiredEquipmentNames?.Count ?? 0;
            var resolvedEquipmentCount = request.ResolveRequiredEquipmentNames().Count;
            _logger.LogInformation(
                "CreateActivity received equipment payload. rawCount={RawCount}, resolvedCount={ResolvedCount}",
                rawEquipmentCount,
                resolvedEquipmentCount);

            var fileUpload = FileUploadMapper.ToFileUpload(request.ActivityImage);
            var createActivityDto = ActivityMapper.ToCreateActivityDto(request, fileUpload);

            var result = await _activityService.CreateActivityAsync(createActivityDto, userId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while creating activity");
            return BadRequest("An unexpected error occurred while creating the activity.");
        }
    }

    /// <summary>
    /// Update an existing activity
    /// </summary>
    [Authorize]
    [HttpPut("activities/{activityId}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ActivityDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateActivity([FromRoute] string activityId, [FromForm] CreateActivityRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        try
        {
            var fileUpload = FileUploadMapper.ToFileUpload(request.ActivityImage);
            var updateActivityDto = ActivityMapper.ToUpdateActivityDto(activityId, request, fileUpload);

            var result = await _activityService.UpdateActivityAsync(updateActivityDto, userId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while updating activity {ActivityId}", activityId);
            return BadRequest("An unexpected error occurred while updating the activity.");
        }
    }

    /// <summary>
    /// Delete given activity
    /// </summary>
    /// <param name="activityId">activityId of activity to delete</param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("activities/{activityId}")]
    [ProducesResponseType(typeof(ActivityDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteActivity([FromRoute] string activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _activityService.DeleteActivityAsync(activityId, userId);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting activity {ActivityId}", activityId);
            return StatusCode(500, "An unexpected error occurred while deleting the activity.");
        }
    }

    /// <summary>
    /// Returns the activity's public share link, generating one on first use.
    /// Any participant can call this.
    /// </summary>
    [Authorize]
    [HttpPost("activities/{activityId}/share")]
    [ProducesResponseType(typeof(ShareLinkDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetOrCreateShareLink([FromRoute] string activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var result = await _activityService.GetOrCreateShareLinkAsync(activityId, userId);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

}
