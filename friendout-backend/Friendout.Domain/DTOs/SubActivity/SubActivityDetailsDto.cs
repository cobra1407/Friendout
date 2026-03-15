using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.DTOs.Localisation;

namespace Friendout.Domain.DTOs.SubActivity;

public class SubActivityDetailsDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime StartTime { get; set; }
    
    public string? Description { get; set; }

    public DateTime EndTime { get; set; }

    public double? Price { get; set; }

    public LocalisationDto? Localisation { get; set; }
    
    public List<ParticipantDto>? Participants { get; set; }
    
}
