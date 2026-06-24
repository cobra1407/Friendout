namespace Friendout.Domain.DTOs.User;

/// <summary>
/// Profile information shown on the preferences screen.
/// Email is intentionally read-only here — it's tied to OAuth login and access whitelisting,
/// so it's not part of UpdateUserProfileDto.
/// </summary>
public record UserProfileDto(
    string Name,
    string? Email,
    string? AvatarUrl,
    bool HasCustomAvatar
);

/// <summary>Payload to update the editable parts of a user's profile (name only for now).</summary>
public record UpdateUserProfileDto(string Name);
