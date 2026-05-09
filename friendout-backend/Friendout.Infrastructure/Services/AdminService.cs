using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Admin;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly FriendoutDbContext _db;
    private readonly ILogger<AdminService> _logger;

    public AdminService(FriendoutDbContext db, ILogger<AdminService> logger)
    {
        _db = db;
        _logger = logger;
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
            return ServiceResult<GuildDto>.Success(new GuildDto(guild.Id, guild.GuildId, guild.Label, guild.CreatedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add allowed guild {GuildId}", dto.GuildId);
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
            _logger.LogError(ex, "Failed to add allowed email {Email}", email);
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
            .Select(r => new AccessRequestDto(r.Id, r.Email, r.Name, r.Message, r.Status, r.CreatedAt, r.ResolvedAt))
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
                new AccessRequestDto(request.Id, request.Email, request.Name, request.Message, request.Status, request.CreatedAt, request.ResolvedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve access request {RequestId}", id);
            return ServiceResult<AccessRequestDto>.Failure("unexpected_error");
        }
    }

    // -------------------------
    // Users
    // -------------------------

    public async Task<List<UserAdminDto>> GetUsersAsync()
    {
        return await _db.Users
            .OrderBy(u => u.CreatedAt)
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
            return ServiceResult<UserAdminDto>.Success(
                new UserAdminDto(user.Id, user.Name, user.Email, user.AvatarUrl, user.Role, user.CreatedAt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update role for user {UserId}", id);
            return ServiceResult<UserAdminDto>.Failure("unexpected_error");
        }
    }
}
