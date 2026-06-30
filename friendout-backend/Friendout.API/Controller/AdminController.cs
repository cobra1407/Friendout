using System.Text;
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
    private readonly ISettingsService _settingsService;

    public AdminController(IAdminService adminService, ISettingsService settingsService)
    {
        _adminService = adminService;
        _settingsService = settingsService;
    }

    private IActionResult MapError(string errorMessage) => errorMessage switch
    {
        "not_found" => NotFound(),
        "guild_already_exists" => Conflict(new { error = errorMessage }),
        "email_already_exists" => Conflict(new { error = errorMessage }),
        "request_already_resolved" => BadRequest(new { error = errorMessage }),
        "last_admin" => BadRequest(new { error = errorMessage }),
        "cannot_delete_self" => BadRequest(new { error = errorMessage }),
        "no_login_method_left" => BadRequest(new { error = errorMessage }),
        _ => StatusCode(500, new { error = errorMessage })
    };

    // -------------------------
    // Access Mode
    // -------------------------

    [HttpGet("admin/access-mode")]
    public async Task<IActionResult> GetAccessMode()
        => Ok(await _adminService.GetAccessModeAsync());

    // -------------------------
    // Access Settings
    // -------------------------

    /// <summary>Returns the current Discord and Google restriction toggles.</summary>
    [HttpGet("admin/access-settings")]
    public async Task<IActionResult> GetAccessSettings()
        => Ok(await _settingsService.GetAccessSettingsAsync());

    /// <summary>Updates the Discord and Google restriction toggles.</summary>
    [HttpPut("admin/access-settings")]
    public async Task<IActionResult> UpdateAccessSettings([FromBody] UpdateAccessSettingsDto dto)
    {
        var result = await _settingsService.UpdateAccessSettingsAsync(dto);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return Ok(result.Data);
    }

    // -------------------------
    // Logs
    // -------------------------

    [HttpGet("admin/logs")]
    public async Task<IActionResult> GetLogs([FromQuery] string? level = null, [FromQuery] int limit = 50,
        [FromQuery] int skip = 0)
        => Ok(await _adminService.GetLogsAsync(level, Math.Clamp(limit, 1, 1000), skip));

    [HttpDelete("admin/logs")]
    public async Task<IActionResult> ClearLogs()
    {
        await _adminService.ClearLogsAsync();
        return NoContent();
    }

    [HttpGet("admin/logs/export")]
    public async Task<IActionResult> ExportLogs()
    {
        var logs = await _adminService.GetLogsAsync(null, 10000);

        var csv = new StringBuilder();
        csv.AppendLine("Id,Level,Category,Message,Exception,CreatedAt");
        foreach (var log in logs)
        {
            csv.AppendLine(
                $"{log.Id},{log.Level},{Escape(log.Category)},{Escape(log.Message)}," +
                $"{Escape(log.Exception ?? "")},{log.CreatedAt:O}"
            );
        }

        var filename = $"friendout-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
    }

    private static string Escape(string value)
        => $"\"{value.Replace("\"", "\"\"")}\"";

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
    public async Task<IActionResult> GetUsers([FromQuery] int skip = 0, [FromQuery] int take = 30)
        => Ok(await _adminService.GetUsersAsync(skip, Math.Clamp(take, 1, 100)));

    [HttpPut("admin/users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDto dto)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, dto);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpDelete("admin/users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _adminService.DeleteUserAsync(id);
        if (!result.IsSuccess) return MapError(result.ErrorMessage);
        return NoContent();
    }
}
