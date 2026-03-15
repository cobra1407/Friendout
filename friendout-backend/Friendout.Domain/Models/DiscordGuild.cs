using System.Text.Json.Serialization;

namespace Friendout.Domain.Models;


// use for oAuth
public class DiscordGuild
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    [JsonPropertyName("owner")]
    public bool? Owner { get; set; }

    [JsonPropertyName("permissions")]
    public long? Permissions { get; set; }

    [JsonPropertyName("permissions_new")]
    public string? PermissionsNew { get; set; }

    [JsonPropertyName("features")]
    public List<string>? Features { get; set; }
}