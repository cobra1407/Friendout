using System.ComponentModel.DataAnnotations;
using Friendout.Domain.Enums;

namespace Friendout.Domain.DTOs.Participant;

public class UserParticipationDto
{
    [Required]
    public string ActivityId { get; set; }

    public string? SubActivityId { get; set; }
    
    public ParticipationStatus Status { get; set; }
}