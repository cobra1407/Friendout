using Friendout.Domain.DTOs.Comment;
using Friendout.Domain.DTOs.Equipment;
using Friendout.Domain.DTOs.Image;
using Friendout.Domain.DTOs.Localisation;
using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.DTOs.SubActivity;

namespace Friendout.Domain.DTOs.Activity;

public class ActivityDetailsDto
{
    // === Activity core ===
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public double? EstimatedPrice { get; set; }
    public double TotalPrice { get; set; }

    public ImageDto? Image { get; set; }
    public LocalisationDto? Localisation { get; set; }

    // === Meta ===
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // === Public sharing ===
    public string? ShareToken { get; set; }

    // === User MainActivity Participation ===
    public UserParticipationDto? UserMainParticipation { get; set; }
    
    // === User SubActivity Participations
    public List<UserParticipationDto> UserSubActivitiesParticipations { get; set; } = [];

    // === Participants (main activity) ===
    public List<ParticipantDto> Participants { get; set; } = [];
    
    // === Equipments ===
    public List<EquipmentDto> RequiredEquipments { get; set; } = [];
    
    // === User Equipements ===
    public List<UserEquipmentDto> UserEquipments { get; set; } = [];

    // === Sub activities ===
    public List<SubActivityDetailsDto> SubActivities { get; set; } = [];

    // === Comments ===
    public List<CommentDto> Comments { get; set; } = [];
}