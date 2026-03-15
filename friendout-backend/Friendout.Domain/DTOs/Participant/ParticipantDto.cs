using Friendout.Domain.Enums;

namespace Friendout.Domain.DTOs.Participant;

public class ParticipantDto
{
    public required string ParticipationId { get; set; }
    public required string Username { get; set; }
    public string? AvatarUrl { get; set; }
    
    public string? SubActivityId { get; set; }
    
    public ParticipationStatus ParticipationStatus { get; set; }

}