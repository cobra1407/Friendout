namespace Friendout.Domain.DTOs.Equipment;

/// <summary>
/// DTO représentant le statut de possession d'équipement par un utilisateur pour une activité
/// </summary>
public class UserEquipmentDto
{
    public string EquipmentId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int RequiredQuantity { get; set; }
    public int Quantity { get; set; }
}
