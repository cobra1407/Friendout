namespace Friendout.Domain.DTOs.Admin;

/// <summary>Represents the current access restriction settings.</summary>
public record AccessSettingsDto(
    bool DiscordRestricted,
    bool GoogleRestricted
);

/// <summary>Payload to update access restriction settings.</summary>
public record UpdateAccessSettingsDto(
    bool DiscordRestricted,
    bool GoogleRestricted
);
