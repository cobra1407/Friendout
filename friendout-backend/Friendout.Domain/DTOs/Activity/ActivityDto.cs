using Friendout.Domain.DTOs.Image;
using Friendout.Domain.DTOs.Localisation;
using Friendout.Domain.DTOs.SubActivity;

public class ActivityDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartAt { get; set; }
    
    public DateTime? EndAt { get; set; }
    
    public List<SubActivityDto>? SubActivities { get; set; }
    public LocalisationDto? Localisation{ get; set; }

    public double? EstimatedPrice { get; set; }
    public ImageDto? Image { get; set; }
    public string CreatedBy { get; set; } = null!;
    
    public int NbParticipants { get; set; }
    public bool HasEquipment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}