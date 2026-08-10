using Friendout.Domain.DTOs.Activity;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace friendout_backend.Controller;

/// <summary>
/// Public, unauthenticated endpoint for viewing an activity shared via its "/share/{token}" link
/// (Komoot-style sharing). Deliberately kept separate from <see cref="ActivityController"/> so the
/// anonymous surface stays small and easy to audit.
/// </summary>
[ApiController]
[EnableRateLimiting("public-share")]
public class PublicActivityController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<PublicActivityController> _logger;

    public PublicActivityController(IActivityService activityService, ILogger<PublicActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the read-only public view of an activity for a given share token.
    /// No authentication required.
    /// </summary>
    [HttpGet("public/activities/{shareToken}")]
    [ProducesResponseType(typeof(PublicActivityDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByShareToken([FromRoute] string shareToken)
    {
        var result = await _activityService.GetPublicActivityAsync(shareToken);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        return Ok(result.Data);
    }
}
