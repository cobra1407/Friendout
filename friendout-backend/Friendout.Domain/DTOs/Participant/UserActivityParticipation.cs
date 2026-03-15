
namespace Friendout.Domain.DTOs.Participant;

public class UserActivityParticipationDto
{
    
    // Main activity participation
    public UserParticipationDto? UserMainParticipation { get; set; }

    // Sub activity participations
    public List<UserParticipationDto> UserSubActivitiesParticipations { get; set; } = [];
    
    // Main activity participants
    public List<ParticipantDto> MainActivityParticipants { get; set; }
    
    // Sub activity participants
    public List<ParticipantDto> SubActivitiesParticipants { get; set; }
}
