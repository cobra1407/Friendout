namespace Friendout.Domain.DTOs.Participant;

public class UserActivityParticipantsDto
{
    // List of participant for main activity
    public List<ParticipantDto> MainActivityParticipants { get; set; } = [];
    
    // List of participants for sub activity
    public List<ParticipantDto> SubActivityParticipants { get; set; } = [];
}
