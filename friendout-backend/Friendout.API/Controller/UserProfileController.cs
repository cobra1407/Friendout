using System.Security.Claims;
using friendout_backend.Mappers;
using friendout_backend.RequestModels.User;
using Friendout.Domain.DTOs.User;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Controller for managing the authenticated user's own profile
/// (name, email, avatar). Users can only access their own profile.
/// </summary>
[ApiController]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public UserProfileController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    [HttpGet("profile/me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _userService.GetUserProfileAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Updates the authenticated user's editable profile fields (name only for now).
    /// </summary>
    [HttpPut("profile/me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _userService.UpdateUserProfileAsync(userId, dto);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Uploads a custom avatar for the authenticated user.
    /// </summary>
    [HttpPost("profile/me/avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var fileUpload = FileUploadMapper.ToFileUpload(request.Avatar);
        if (fileUpload is null)
            return BadRequest("No file provided.");

        var result = await _userService.UploadAvatarAsync(userId, fileUpload);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    /// <summary>
    /// Resets the authenticated user's avatar back to their original OAuth avatar.
    /// </summary>
    [HttpPost("profile/me/avatar/reset")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ResetAvatar()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found in token");

        var result = await _userService.ResetAvatarAsync(userId);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }
}
