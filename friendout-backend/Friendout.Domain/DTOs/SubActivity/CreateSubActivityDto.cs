using System.ComponentModel.DataAnnotations;

namespace Friendout.Domain.DTOs.SubActivity;

public class CreateSubActivityDto
{
    [MaxLength(191)]
    public string? Id { get; set; }

    [Required, MaxLength(191)]
    public string Name { get; set; } = null!;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    public string? Description { get; set; }

    public double? Price { get; set; }

    public string? Address { get; set; }

    public string? MapLink { get; set; }

    public string? VirtualUrl { get; set; }
}
