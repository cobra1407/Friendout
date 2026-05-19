using Friendout.Domain.Enums;
using Friendout.Domain.Models;

namespace Friendout.Domain.DTOs.Admin;

public record AddGuildDto(string GuildId, string? Label);
public record AddEmailDto(string Email);
public record UpdateUserRoleDto(UserRole Role);
public record ResolveAccessRequestDto(AccessRequestStatus Status);
public record SubmitAccessRequestDto(string Email, string? Message);
