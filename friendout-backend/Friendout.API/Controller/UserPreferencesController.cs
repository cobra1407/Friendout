using System.Security.Claims;
using Friendout.Domain.DTOs.Preferences;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller for managing the authenticated user's own preferences
/// (locale + notification channels). Users can only access their own preferences.
/// </summary>
[ApiController]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly IUserPreferencesService _preferencesService;

    public UserPreferencesController(IUserPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Returns the authenticated user's preferences, defaulting to sensible values
    /// if the user has never saved any.
    /// </summary>
    [HttpGet("preferences/me")]
    [ProducesResponseType(typeof(UserPreferencesDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyPreferences()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var preferences = await _preferencesService.GetMyPreferencesAsync(userId);
        return Ok(preferences);
    }

    /// <summary>
    /// Updates the authenticated user's preferences.
    /// </summary>
    [HttpPut("preferences/me")]
    [ProducesResponseType(typeof(UserPreferencesDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateUserPreferences([FromBody] UpdateUserPreferencesDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _preferencesService.UpdateUserPreferencesAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }
}
