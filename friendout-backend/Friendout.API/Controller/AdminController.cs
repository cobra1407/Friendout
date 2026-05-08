using Friendout.Domain.DTOs.Admin;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace friendout_backend.Controller;

/// <summary>
/// Administration endpoints — restricted to users with the Admin role.
/// Manages allowed guilds, allowed emails, access requests, and user roles.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private IActionResult MapError(string errorMessage) => errorMessage switch
    {
        "not_found"                => NotFound(),
        "guild_already_exists"     => Conflict(new { error = errorMessage }),
        "email_already_exists"     => Conflict(new { error = errorMessage }),
        "request_already_resolved" => BadRequest(new { error = errorMessage }),
        _                          => StatusCode(500, new { error = errorMessage })
    };

    // -------------------------
    // Guilds
    // -------------------------

    [HttpGet("admin/allowed-guilds")]
    public async Task<IActionResult> GetAllowedGuilds()
        => Ok(await _adminService.GetAllowedGuildsAsync());

    [HttpPost("admin/allowed-guilds")]
    public async Task<IActionResult> AddAllowedGuild([FromBody] AddGuildDto dto)
    {
        var result = await _adminService.AddAllowedGuildAsync(dto);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return Created(string.Empty, result.Data);
    }

    [HttpDelete("admin/allowed-guilds/{id:int}")]
    public async Task<IActionResult> DeleteAllowedGuild(int id)
    {
        var result = await _adminService.DeleteAllowedGuildAsync(id);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return NoContent();
    }

    // -------------------------
    // Emails
    // -------------------------

    [HttpGet("admin/allowed-emails")]
    public async Task<IActionResult> GetAllowedEmails()
        => Ok(await _adminService.GetAllowedEmailsAsync());

    [HttpPost("admin/allowed-emails")]
    public async Task<IActionResult> AddAllowedEmail([FromBody] AddEmailDto dto)
    {
        var result = await _adminService.AddAllowedEmailAsync(dto);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return Created(string.Empty, result.Data);
    }

    [HttpDelete("admin/allowed-emails/{id:int}")]
    public async Task<IActionResult> DeleteAllowedEmail(int id)
    {
        var result = await _adminService.DeleteAllowedEmailAsync(id);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return NoContent();
    }

    // -------------------------
    // Access Requests
    // -------------------------

    [HttpGet("admin/access-requests")]
    public async Task<IActionResult> GetAccessRequests([FromQuery] string? status = null)
        => Ok(await _adminService.GetAccessRequestsAsync(status));

    [HttpPut("admin/access-requests/{id:int}")]
    public async Task<IActionResult> ResolveAccessRequest(int id, [FromBody] ResolveAccessRequestDto dto)
    {
        var result = await _adminService.ResolveAccessRequestAsync(id, dto);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return Ok(result.Data);
    }

    // -------------------------
    // Users
    // -------------------------

    [HttpGet("admin/users")]
    public async Task<IActionResult> GetUsers()
        => Ok(await _adminService.GetUsersAsync());

    [HttpPut("admin/users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDto dto)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, dto);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return Ok(result.Data);
    }
}
