using System.ComponentModel.DataAnnotations;

namespace Friendout.Domain.DTOs.EquipmentList;

/// <summary>
/// Payload used to update an existing equipment list. Items are fully replaced.
/// </summary>
public class UpdateEquipmentListDto
{
    [Required]
    [MaxLength(191)]
    public string Name { get; set; } = null!;

    public List<string> Items { get; set; } = new();
}
