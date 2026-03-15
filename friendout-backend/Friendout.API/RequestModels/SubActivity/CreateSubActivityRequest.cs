using System.ComponentModel.DataAnnotations;

namespace friendout_backend.RequestModels.SubActivity;

public class CreateSubActivityRequest
{
    [MaxLength(191)]
    public string? Id { get; set; }

    [Required, MaxLength(191)]
    public string Name { get; set; } = null!;

    [Required]
    public string StartTime { get; set; } = null!;

    public string? EndTime { get; set; }

    public string? Description { get; set; }

    public double? Price { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? MapLink { get; set; }

    [MaxLength(500)]
    public string? VirtualUrl { get; set; }
}
