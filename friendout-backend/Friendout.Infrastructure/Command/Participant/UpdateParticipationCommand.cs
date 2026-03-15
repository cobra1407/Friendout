using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Friendout.Domain.Enums;

namespace Friendout.Infrastructure.Command.Participant;

public class UpdateParticipationCommand
{
    [Required]
    public string ActivityId { get; init; } = null!;
    public ParticipationStatus Status { get; init; }
    public IReadOnlyList<string>? SubActivityIds  { get; init; }
}
