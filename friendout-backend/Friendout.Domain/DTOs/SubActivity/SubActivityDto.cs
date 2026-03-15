using Friendout.Domain.DTOs.Localisation;

namespace Friendout.Domain.DTOs.SubActivity;

public class SubActivityDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public double? Price { get; set; }

    public LocalisationDto? Localisation { get; set; }
}
