namespace Friendout.Domain.DTOs.Equipment;

public class EquipmentDto
{
    public string EquipmentId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    public int Quantity { get; set; }
}
