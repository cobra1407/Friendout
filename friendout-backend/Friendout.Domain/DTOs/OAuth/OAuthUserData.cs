using Friendout.Domain.Enums;

namespace Friendout.Domain.DTOs.OAuth;

public class OAuthUserData
{
    public required ProviderEnum Provider { get; init; }
    public required string ProviderUserId { get; init; }

    public required string Username { get; init; }
    public string? Email { get; init; }
    public string? AvatarUrl { get; init; }
}
