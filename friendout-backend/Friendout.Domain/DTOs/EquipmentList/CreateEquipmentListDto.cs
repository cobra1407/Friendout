using System.ComponentModel.DataAnnotations;

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
    /// Equipment names to store in the list. Duplicates and blank entries are ignored.
    /// </summary>
    public List<string> Items { get; set; } = new();
}
