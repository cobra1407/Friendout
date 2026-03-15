using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.Equipment;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Interface pour le service de gestion de l'équipement
/// </summary>
public interface IEquipmentService
{
    /// <summary>
    /// Récupère l'équipement de l'utilisateur pour une activité donnée
    /// </summary>
    /// <param name="activityId">ID de l'activité</param>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>Liste des équipements avec leur statut de possession par l'utilisateur</returns>
    Task<ServiceResult<List<UserEquipmentDto>>> GetUserEquipmentForActivityAsync(string activityId, string userId);

    /// <summary>
    /// Définit la quantité d’un équipement possédée par un utilisateur.
    /// Si la quantité est supérieure à 0, l’équipement est ajouté ou mis à jour.
    /// Si la quantité est égale ou inférieure à 0, l’équipement est supprimé.
    /// </summary>
    /// <param name="activityId">Identifiant de l'activité</param>
    /// <param name="equipmentId">Identifiant de l’équipement</param>
    /// <param name="userId">Identifiant de l’utilisateur</param>
    /// <param name="quantity">
    /// Quantité possédée par l’utilisateur.
    /// Une valeur &lt;= 0 entraîne la suppression de l’équipement.
    /// </param>
    /// <returns>
    /// <see cref="ServiceResult{T}"/> indiquant si l’opération a réussi.
    /// </returns>
    Task<ServiceResult<List<UserEquipmentDto>>> SetUserEquipmentAsync(string activityId, string equipmentId, string userId, int quantity);
    
    /// <summary>
    /// Vérifie si l'utilisateur possède un équipement spécifique
    /// </summary>
    /// <param name="equipmentId">ID de l'équipement</param>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>True si l'utilisateur possède l'équipement, false sinon</returns>
    Task<ServiceResult<bool>> UserHasEquipmentAsync(string equipmentId, string userId);
}
