namespace Friendout.Domain.DTOs.EquipmentList;

/// <summary>
/// DTO representing an equipment list, including its items.
/// </summary>
public class EquipmentListDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public List<string> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
