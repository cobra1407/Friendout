using System.ComponentModel.DataAnnotations;
using Friendout.Domain.Constants;

namespace Friendout.Domain.DTOs.EquipmentList;

/// <summary>
/// Payload used to update an existing equipment list. Items are fully replaced.
/// </summary>
public class UpdateEquipmentListDto
{
    [Required]
    [MaxLength(191)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Icon key. Must be one of <see cref="EquipmentListIcons.AllowedKeys"/>; falls back
    /// to <see cref="EquipmentListIcons.Default"/> when omitted or unrecognized.
    /// </summary>
    public string? Icon { get; set; }

    public List<string> Items { get; set; } = new();
}
