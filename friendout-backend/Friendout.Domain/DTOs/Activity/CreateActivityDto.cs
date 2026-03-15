using System.ComponentModel.DataAnnotations;
using Friendout.Domain.DTOs.SubActivity;
using Friendout.Domain.Models;

namespace Friendout.Domain.DTOs.Activity
{
    /// <summary>
    /// DTO utilisé pour créer une nouvelle activité.
    /// Indépendant de la couche web.
    /// </summary>
    public class CreateActivityDto
    {
        [Required, MaxLength(191)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required, MaxLength(191)]
        public string Time { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? MapLink { get; set; }

        [MaxLength(500)]
        public string? VirtualUrl { get; set; }
        
        public double? EstimatedPrice { get; set; }

        public List<string> RequiredEquipmentNames { get; set; } = new();

        public List<CreateSubActivityDto> SubActivities { get; set; } = new();
        
        public FileUpload? ActivityImage { get; set; }
    }
}
