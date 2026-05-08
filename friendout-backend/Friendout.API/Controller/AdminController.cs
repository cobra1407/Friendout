using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace friendout_backend.Controller;

/// <summary>
/// Administration endpoints — restricted to users with the Admin role.
/// Manages allowed guilds, allowed emails, access requests, and user roles.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly FriendoutDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="db">The Friendout database context.</param>
    public AdminController(FriendoutDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all allowed guilds.
    /// </summary>
    /// <returns>A list of allowed guilds ordered by creation date.</returns>
    [HttpGet("admin/allowed-guilds")]
    public async Task<IActionResult> GetAllowedGuilds()
    {
        var guilds = await _db.AllowedGuilds
            .OrderBy(g => g.CreatedAt)
            .Select(g => new GuildDto(g.Id, g.GuildId, g.Label, g.CreatedAt))
            .ToListAsync();
        return Ok(guilds);
    }

    /// <summary>
    /// Adds a new guild to the allowed guild list.
    /// </summary>
    /// <param name="dto">The guild details to add.</param>
    /// <returns>The created guild data.</returns>
    [HttpPost("admin/allowed-guilds")]
    public async Task<IActionResult> AddAllowedGuild([FromBody] AddGuildDto dto)
    {
        if (await _db.AllowedGuilds.AnyAsync(g => g.GuildId == dto.GuildId))
            return Conflict(new { error = "guild_already_exists" });

        var guild = new AllowedGuild { GuildId = dto.GuildId, Label = dto.Label };
        _db.AllowedGuilds.Add(guild);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new GuildDto(guild.Id, guild.GuildId, guild.Label, guild.CreatedAt));
    }

    /// <summary>
    /// Deletes an allowed guild by its ID.
    /// </summary>
    /// <param name="id">The ID of the guild to delete.</param>
    /// <returns>No content if successful, NotFound if the guild does not exist.</returns>
    [HttpDelete("admin/allowed-guilds/{id:int}")]
    public async Task<IActionResult> DeleteAllowedGuild(int id)
    {
        var guild = await _db.AllowedGuilds.FindAsync(id);
        if (guild is null) return NotFound();

        _db.AllowedGuilds.Remove(guild);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    /// <summary>
    /// Retrieves all allowed emails.
    /// </summary>
    /// <returns>A list of allowed emails ordered by creation date.</returns>
    [HttpGet("admin/allowed-emails")]
    public async Task<IActionResult> GetAllowedEmails()
    {
        var emails = await _db.AllowedEmails
            .OrderBy(e => e.CreatedAt)
            .Select(e => new EmailDto(e.Id, e.Email, e.CreatedAt))
            .ToListAsync();
        return Ok(emails);
    }

    /// <summary>
    /// Adds a new email to the allowed email list.
    /// </summary>
    /// <param name="dto">The email details to add.</param>
    /// <returns>The created email data.</returns>
    [HttpPost("admin/allowed-emails")]
    public async Task<IActionResult> AddAllowedEmail([FromBody] AddEmailDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.AllowedEmails.AnyAsync(e => e.Email == email))
            return Conflict(new { error = "email_already_exists" });

        var entry = new AllowedEmail { Email = email };
        _db.AllowedEmails.Add(entry);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new EmailDto(entry.Id, entry.Email, entry.CreatedAt));
    }

    /// <summary>
    /// Deletes an allowed email by its ID.
    /// </summary>
    /// <param name="id">The ID of the email to delete.</param>
    /// <returns>No content if successful, NotFound if the email does not exist.</returns>
    [HttpDelete("admin/allowed-emails/{id:int}")]
    public async Task<IActionResult> DeleteAllowedEmail(int id)
    {
        var entry = await _db.AllowedEmails.FindAsync(id);
        if (entry is null) return NotFound();

        _db.AllowedEmails.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    /// <summary>
    /// Retrieves access requests, optionally filtered by status.
    /// </summary>
    /// <param name="status">Optional access request status to filter by.</param>
    /// <returns>A list of access requests ordered by most recent.</returns>
    [HttpGet("admin/access-requests")]
    public async Task<IActionResult> GetAccessRequests([FromQuery] string? status = null)
    {
        var query = _db.AccessRequests.AsQueryable();

        if (Enum.TryParse<AccessRequestStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(r => r.Status == parsedStatus);

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AccessRequestDto(r.Id, r.Email, r.Name, r.Message, r.Status, r.CreatedAt, r.ResolvedAt))
            .ToListAsync();

        return Ok(requests);
    }

    /// <summary>
    /// Resolves an access request by updating its status.
    /// </summary>
    /// <param name="id">The ID of the access request to resolve.</param>
    /// <param name="dto">The resolution details including the new status.</param>
    /// <returns>The updated access request data.</returns>
    [HttpPut("admin/access-requests/{id:int}")]
    public async Task<IActionResult> ResolveAccessRequest(int id, [FromBody] ResolveAccessRequestDto dto)
    {
        var request = await _db.AccessRequests.FindAsync(id);
        if (request is null) return NotFound();

        request.Status = dto.Status;
        request.ResolvedAt = DateTime.UtcNow;

        // If approved, automatically add the email to the allowed list.
        if (dto.Status == AccessRequestStatus.Approved)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (!await _db.AllowedEmails.AnyAsync(e => e.Email == email))
                _db.AllowedEmails.Add(new AllowedEmail { Email = email });
        }

        await _db.SaveChangesAsync();
        return Ok(new AccessRequestDto(request.Id, request.Email, request.Name, request.Message, request.Status, request.CreatedAt, request.ResolvedAt));
    }


    /// <summary>
    /// Retrieves all users for administration purposes.
    /// </summary>
    /// <returns>A list of users ordered by creation date.</returns>
    [HttpGet("admin/users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserAdminDto(u.Id, u.Name, u.Email, u.AvatarUrl, u.Role, u.CreatedAt))
            .ToListAsync();
        return Ok(users);
    }

    /// <summary>
    /// Updates the role of an existing user.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="dto">The role update details.</param>
    /// <returns>The updated user data.</returns>
    [HttpPut("admin/users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new UserAdminDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Role, user.CreatedAt));
    }
}
