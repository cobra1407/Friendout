using Friendout.Domain.Enums;
using Friendout.Domain.Models;

namespace Friendout.Domain.DTOs.Admin;

public record GuildDto(int Id, string GuildId, string? Label, DateTime CreatedAt);
public record EmailDto(int Id, string Email, DateTime CreatedAt);
public record UserAdminDto(string Id, string Name, string? Email, string? AvatarUrl, UserRole Role, DateTime CreatedAt);
public record AccessRequestDto(int Id, string Email, string? Message, AccessRequestStatus Status, DateTime CreatedAt, DateTime? ResolvedAt);

// Access mode DTO with summary counts for admin dashboard.
// IsDiscordOpenMode: Discord restriction toggle is off — anyone with a Discord account can log in.
// IsDiscordRestrictionLocksEveryone: Discord restriction toggle is on but no guild is configured yet,
// so the allowlist is empty and the Discord login check blocks every user (admins included).
// IsGoogleOpenMode: Google restriction toggle is off — anyone with a Google account can log in.
// IsGoogleRestrictionLocksEveryone: Google restriction toggle is on but no email is configured yet,
// so the whitelist is empty and the Google login check blocks every user (admins included).
public record AccessModeDto(
    bool IsDiscordOpenMode,
    bool IsDiscordRestrictionLocksEveryone,
    bool IsGoogleOpenMode,
    bool IsGoogleRestrictionLocksEveryone,
    int GuildCount,
    int EmailCount);
public record AppLogDto(int Id, string Level, string Category, string Message, string? Exception, DateTime CreatedAt);
