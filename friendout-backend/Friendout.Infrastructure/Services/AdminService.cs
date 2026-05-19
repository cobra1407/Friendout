using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly FriendoutDbContext _db;
    private readonly IAppLogService _appLog;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminService(FriendoutDbContext db, IAppLogService appLog, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _appLog = appLog;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Returns the (id, name) of the currently authenticated admin.</summary>
    private async Task<(string id, string name)> GetActorAsync()
    {
        var actorId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        if (actorId == "unknown") return (actorId, "unknown");
        var actor = await _db.Users.FindAsync(actorId);
        return (actorId, actor?.Name ?? actorId);
    }

    // -------------------------
    // Access Mode
    // -------------------------

    public async Task<AccessModeDto> GetAccessModeAsync()
    {
        var guildCount = await _db.AllowedGuilds.CountAsync();
        var emailCount = await _db.AllowedEmails.CountAsync();
        return new AccessModeDto(IsOpenMode: guildCount == 0, GuildCount: guildCount, EmailCount: emailCount);
    }

    // -------------------------
    // Logs
    // -------------------------

    public async Task<List<AppLogDto>> GetLogsAsync(string? level, int limit)
    {
        var query = _db.AppLogs.AsQueryable();

        if (Enum.TryParse<AppLogLevel>(level, ignoreCase: true, out var parsedLevel))
            query = query.Where(l => l.Level == parsedLevel);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .Select(l => new AppLogDto(l.Id, l.Level.ToString(), l.Category, l.Message, l.Exception, l.CreatedAt))
            .ToListAsync();
    }

    public async Task ClearLogsAsync()
    {
        await _db.AppLogs.ExecuteDeleteAsync();
    }

    // -------------------------
    // Guilds
    // -------------------------

    public async Task<List<GuildDto>> GetAllowedGuildsAsync()
    {
        return await _db.AllowedGuilds
            .OrderBy(g => g.CreatedAt)
            .Select(g => new GuildDto(g.Id, g.GuildId, g.Label, g.CreatedAt))
            .ToListAsync();
    }

    public async Task<ServiceResult<GuildDto>> AddAllowedGuildAsync(AddGuildDto dto)
    {
        if (await _db.AllowedGuilds.AnyAsync(g => g.GuildId == dto.GuildId))
            return ServiceResult<GuildDto>.Failure("guild_already_exists");

        var guild = new AllowedGuild { GuildId = dto.GuildId, Label = dto.Label };
        _db.AllowedGuilds.Add(guild);

        try
        {
            await _db.SaveChangesAsync();
            var (actorId, actorName) = await GetActorAsync();
            await _appLog.LogInfoAsync("Admin", $"{actorName} ({actorId}) added guild {dto.GuildId} ({dto.Label})");
            return ServiceResult<GuildDto>.Success(new GuildDto(guild.Id, guild.GuildId, guild.Label, guild.CreatedAt));
        }
        catch (Exception ex)
        {
            await _appLog.LogErrorAsync("Admin", $"Failed to add guild {dto.GuildId}", ex);
            return ServiceResult<GuildDto>.Failure("unexpected_error");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAllowedGuildAsync(int id)
    {
        var guild = await _db.AllowedGuilds.FindAsync(id);
        if (guild is null)
            return ServiceResult<bool>.Failure("not_found");

        _db.AllowedGuilds.Remove(guild);
        await _db.SaveChangesAsync();
        var (actorId, actorName) = await GetActorAsync();
        await _appLog.LogInfoAsync("Admin", $"{actorName} ({actorId}) removed guild {guild.GuildId} ({guild.Label})");
        return ServiceResult<bool>.Success(true);
    }

    // -------------------------
    // Emails
    // -------------------------

    public async Task<List<EmailDto>> GetAllowedEmailsAsync()
    {
        return await _db.AllowedEmails
            .OrderBy(e => e.CreatedAt)
            .Select(e => new EmailDto(e.Id, e.Email, e.CreatedAt))
            .ToListAsync();
    }

    public async Task<ServiceResult<EmailDto>> AddAllowedEmailAsync(AddEmailDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.AllowedEmails.AnyAsync(e => e.Email == email))
            return ServiceResult<EmailDto>.Failure("email_already_exists");

        var entry = new AllowedEmail { Email = email };
        _db.AllowedEmails.Add(entry);

        try
        {
            await _db.SaveChangesAsync();
            return ServiceResult<EmailDto>.Success(new EmailDto(entry.Id, entry.Email, entry.CreatedAt));
        }
        catch (Exception ex)
        {
            await _appLog.LogErrorAsync("Admin", $"Failed to add email {email}", ex);
            return ServiceResult<EmailDto>.Failure("unexpected_error");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAllowedEmailAsync(int id)
    {
        var entry = await _db.AllowedEmails.FindAsync(id);
        if (entry is null)
            return ServiceResult<bool>.Failure("not_found");

        _db.AllowedEmails.Remove(entry);
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    // -------------------------
    // Access Requests
    // -------------------------

    public async Task<List<AccessRequestDto>> GetAccessRequestsAsync(string? status)
    {
        var query = _db.AccessRequests.AsQueryable();

        if (Enum.TryParse<AccessRequestStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(r => r.Status == parsedStatus);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AccessRequestDto(r.Id, r.Email, r.Message, r.Status, r.CreatedAt, r.ResolvedAt))
            .ToListAsync();
    }

    public async Task<ServiceResult<AccessRequestDto>> ResolveAccessRequestAsync(int id, ResolveAccessRequestDto dto)
    {
        var request = await _db.AccessRequests.FindAsync(id);
        if (request is null)
            return ServiceResult<AccessRequestDto>.Failure("not_found");

        // Security: prevent re-processing an already resolved request.
        if (request.Status != AccessRequestStatus.Pending)
            return ServiceResult<AccessRequestDto>.Failure("request_already_resolved");

        request.Status = dto.Status;
        request.ResolvedAt = DateTime.UtcNow;

        // If approved, automatically add the email to the allowed list.
        if (dto.Status == AccessRequestStatus.Approved)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (!await _db.AllowedEmails.AnyAsync(e => e.Email == email))
                _db.AllowedEmails.Add(new AllowedEmail { Email = email });
        }

        try
        {
            await _db.SaveChangesAsync();
            return ServiceResult<AccessRequestDto>.Success(
                new AccessRequestDto(request.Id, request.Email, request.Message, request.Status, request.CreatedAt, request.ResolvedAt));
        }
        catch (Exception ex)
        {
            await _appLog.LogErrorAsync("Admin", $"Failed to resolve access request {id}", ex);
            return ServiceResult<AccessRequestDto>.Failure("unexpected_error");
        }
    }

    public async Task<ServiceResult<bool>> SubmitAccessRequestAsync(SubmitAccessRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        // Reject if a pending request already exists for this email.
        if (await _db.AccessRequests.AnyAsync(r => r.Email == email && r.Status == AccessRequestStatus.Pending))
            return ServiceResult<bool>.Failure("already_pending");

        // Reject if the email is already in the allowed list.
        if (await _db.AllowedEmails.AnyAsync(e => e.Email == email))
            return ServiceResult<bool>.Failure("already_approved");

        // Cap: refuse new requests when too many are already pending.
        // Protects the database from flooding via multiple IPs or generated emails.
        const int MaxPendingRequests = 50;
        if (await _db.AccessRequests.CountAsync(r => r.Status == AccessRequestStatus.Pending) >= MaxPendingRequests)
            return ServiceResult<bool>.Failure("too_many_pending");

        _db.AccessRequests.Add(new AccessRequest
        {
            Email   = email,
            Message = dto.Message?.Trim(),
            Status  = AccessRequestStatus.Pending
        });

        try
        {
            await _db.SaveChangesAsync();
            await _appLog.LogInfoAsync("AccessRequest", $"New access request from {email}");
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _appLog.LogErrorAsync("AccessRequest", $"Failed to save access request for {email}", ex);
            return ServiceResult<bool>.Failure("unexpected_error");
        }
    }

    // -------------------------
    // Users
    // -------------------------

    public async Task<List<UserAdminDto>> GetUsersAsync()
    {
        return await _db.Users
            .OrderBy(u => u.Role == UserRole.Admin ? 0 : 1)
            .ThenBy(u => u.CreatedAt)
            .Select(u => new UserAdminDto(u.Id, u.Name, u.Email, u.AvatarUrl, u.Role, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<ServiceResult<UserAdminDto>> UpdateUserRoleAsync(string id, UpdateUserRoleDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return ServiceResult<UserAdminDto>.Failure("not_found");

        // Prevent demoting the last admin — at least one admin must always exist.
        if (dto.Role == UserRole.User && user.Role == UserRole.Admin)
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
                return ServiceResult<UserAdminDto>.Failure("last_admin");
        }

        user.Role = dto.Role;
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
            var (actorId, actorName) = await GetActorAsync();
            await _appLog.LogInfoAsync("Admin", $"{actorName} ({actorId}) changed {user.Name} ({id}) role to {dto.Role}");
            return ServiceResult<UserAdminDto>.Success(
                new UserAdminDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Role, user.CreatedAt));
        }
        catch (Exception ex)
        {
            await _appLog.LogErrorAsync("Admin", $"Failed to update role for user {id}", ex);
            return ServiceResult<UserAdminDto>.Failure("unexpected_error");
        }
    }
}
