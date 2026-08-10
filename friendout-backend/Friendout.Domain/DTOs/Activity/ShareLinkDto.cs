namespace Friendout.Domain.DTOs.Activity;

/// <summary>
/// Returned when generating (or fetching the existing) public share link for an activity.
/// </summary>
public class ShareLinkDto
{
    public string ShareToken { get; set; } = null!;
}
