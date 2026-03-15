using Friendout.Domain.DTOs.SubActivity;
using Friendout.Domain.Models;

namespace Friendout.Domain.DTOs.Activity;

public class UpdateActivityDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Address { get; set; }
    public string? MapLink { get; set; }
    public string? VirtualUrl { get; set; }
    public double? EstimatedPrice { get; set; }
    public List<string> RequiredEquipmentNames { get; set; } = new();
    public List<CreateSubActivityDto> SubActivities { get; set; } = new();
    public FileUpload? ActivityImage { get; set; }
}
