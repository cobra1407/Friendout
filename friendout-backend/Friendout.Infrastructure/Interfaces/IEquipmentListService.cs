using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.EquipmentList;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Interface for managing user-owned reusable equipment lists.
/// </summary>
public interface IEquipmentListService
{
    /// <summary>
    /// Returns all equipment lists owned by the given user, ordered by name.
    /// </summary>
    /// <param name="userId">The owner's user ID.</param>
    Task<ServiceResult<List<EquipmentListDto>>> GetUserEquipmentListsAsync(string userId);

    /// <summary>
    /// Returns a single equipment list, scoped to its owner.
    /// </summary>
    /// <param name="equipmentListId">The equipment list ID.</param>
    /// <param name="userId">The owner's user ID.</param>
    Task<ServiceResult<EquipmentListDto>> GetEquipmentListByIdAsync(string equipmentListId, string userId);

    /// <summary>
    /// Creates a new equipment list for the given user.
    /// </summary>
    /// <param name="userId">The owner's user ID.</param>
    /// <param name="request">The list name and items.</param>
    Task<ServiceResult<EquipmentListDto>> CreateEquipmentListAsync(string userId, CreateEquipmentListDto request);

    /// <summary>
    /// Updates an existing equipment list owned by the given user. Items are fully replaced.
    /// </summary>
    /// <param name="equipmentListId">The equipment list ID.</param>
    /// <param name="userId">The owner's user ID.</param>
    /// <param name="request">The new name and items.</param>
    Task<ServiceResult<EquipmentListDto>> UpdateEquipmentListAsync(string equipmentListId, string userId, UpdateEquipmentListDto request);

    /// <summary>
    /// Deletes an equipment list owned by the given user.
    /// </summary>
    /// <param name="equipmentListId">The equipment list ID.</param>
    /// <param name="userId">The owner's user ID.</param>
    Task<ServiceResult<bool>> DeleteEquipmentListAsync(string equipmentListId, string userId);
}
