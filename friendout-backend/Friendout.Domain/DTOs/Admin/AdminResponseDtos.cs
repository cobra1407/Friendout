using Friendout.Domain.Enums;
using Friendout.Domain.Models;

namespace Friendout.Domain.DTOs.Admin;

public record GuildDto(int Id, string GuildId, string? Label, DateTime CreatedAt);
public record EmailDto(int Id, string Email, DateTime CreatedAt);
public record UserAdminDto(string Id, string Name, string? Email, string? AvatarUrl, UserRole Role, DateTime CreatedAt);
public record AccessRequestDto(int Id, string Email, string? Name, string? Message, AccessRequestStatus Status, DateTime CreatedAt, DateTime? ResolvedAt);

// Access mode DTO with summary counts for admin dashboard.
public record AccessModeDto(bool IsOpenMode, int GuildCount, int EmailCount);
