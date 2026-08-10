using Friendout.Domain.DTOs.Image;
using Friendout.Domain.DTOs.Localisation;

namespace Friendout.Domain.DTOs.Activity;

/// <summary>
/// Read-only view of an activity exposed via its public share link.
/// Intentionally excludes participant identities, comments, user equipment state,
/// and anything else not meant to be visible to an anonymous visitor. Participants
/// are exposed as counts only (see <see cref="ParticipantsCount"/>), never as a list
/// of names, since this page is reachable by anyone with the link.
/// </summary>
public class PublicActivityDto
{
    /// <summary>
    /// Real activity id. Not sensitive on its own (a non-enumerable GUID) — only used
    /// so an already-authenticated visitor can be redirected to the private detail page.
    /// </summary>
    public string ActivityId { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public double? EstimatedPrice { get; set; }

    public ImageDto? Image { get; set; }
    public LocalisationDto? Localisation { get; set; }

    /// <summary>
    /// Display name of the organizer only — no id, email, or other user data.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    public PublicParticipantsCountDto ParticipantsCount { get; set; } = new();

    public List<PublicSubActivityDto> SubActivities { get; set; } = [];
    public List<string> RequiredEquipmentNames { get; set; } = [];
}

public class PublicParticipantsCountDto
{
    public int Participating { get; set; }
    public int Maybe { get; set; }
    public int NotParticipating { get; set; }
}

public class PublicSubActivityDto
{
    public string Name { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public double? Price { get; set; }
    public LocalisationDto? Localisation { get; set; }
}
