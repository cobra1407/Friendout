using System.ComponentModel.DataAnnotations;
using Friendout.Domain.Constants;

namespace Friendout.Domain.DTOs.EquipmentList;

/// <summary>
/// Payload used to create a new equipment list.
/// </summary>
public class CreateEquipmentListDto
{
    [Required]
    [MaxLength(191)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Icon key. Must be one of <see cref="EquipmentListIcons.AllowedKeys"/>; falls back
    /// to <see cref="EquipmentListIcons.Default"/> when omitted or unrecognized.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Equipment names to store in the list. Duplicates and blank entries are ignored.
    /// </summary>
    public List<string> Items { get; set; } = new();
}
