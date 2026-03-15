using Friendout.Domain.Enums;

namespace Friendout.Domain.DTOs.Participant;

public class UpdateParticipationDto
{
    public List<string>? SubActivityIds  { get; set; }
    public ParticipationStatus Status { get; set; }
}