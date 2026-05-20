using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Equipment;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Service pour la gestion de l'équipement des utilisateurs
/// </summary>
public class EquipmentService : IEquipmentService
{
    private readonly FriendoutDbContext _friendoutDbContext;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(FriendoutDbContext context, ILogger<EquipmentService> logger)
    {
        _friendoutDbContext = context;
        _logger = logger;
    }
    
    private async Task<ServiceResult<List<UserEquipmentDto>>> GetUserEquipmentsAsync(string userId, string activityId)
    {
        var equipments = await _friendoutDbContext.UserEquipment
            .Include(ue => ue.Equipment)
            .Where(ue => ue.UserId == userId && ue.ActivityId == activityId)
            .Select(ue => new UserEquipmentDto
            {
                EquipmentId = ue.EquipmentId,
                Name = ue.Equipment.Name,
                Description = ue.Equipment.Description,
                Quantity = ue.Quantity
            })
            .ToListAsync();

        return ServiceResult<List<UserEquipmentDto>>.Success(equipments);
    }

    
    public async Task<ServiceResult<List<UserEquipmentDto>>> GetUserEquipmentForActivityAsync(string activityId, string userId)
    {
        try
        {
            // Vérifier que l'activité existe
            var activityExists = await _friendoutDbContext.Activities
                .AnyAsync(a => a.Id == activityId);

            if (!activityExists)
            {
                return ServiceResult<List<UserEquipmentDto>>.Failure("Activity not found");
            }

            // Récupérer uniquement les équipements que l'utilisateur possède pour cette activité
            var userEquipments = await _friendoutDbContext.UserEquipment
                .Where(ue => ue.UserId == userId && ue.ActivityId == activityId)
                .Include(ue => ue.Equipment) // inclure les détails de l'équipement
                .ToListAsync();

            // Mapper vers le DTO
            var result = userEquipments.Select(ue => new UserEquipmentDto
            {
                EquipmentId = ue.EquipmentId,
                Name = ue.Equipment.Name,
                Description = ue.Equipment.Description,
                Quantity = ue.Quantity,
                RequiredQuantity = 0 // optionnel : si tu veux afficher combien est requis, il faut joindre ActivityEquipment
            }).ToList();

            return ServiceResult<List<UserEquipmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve user equipment for activity {activityId}");
            return ServiceResult<List<UserEquipmentDto>>.Failure(
                "An error occurred while retrieving user equipment for the activity");
        }
    }
    

      public async Task<ServiceResult<List<UserEquipmentDto>>> SetUserEquipmentAsync(
        string activityId,
        string equipmentId,
        string userId,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(activityId))
            return ServiceResult<List<UserEquipmentDto>>.Failure("ActivityId is required");

        if (string.IsNullOrWhiteSpace(equipmentId))
            return ServiceResult<List<UserEquipmentDto>>.Failure("EquipmentId is required");

        if (string.IsNullOrWhiteSpace(userId))
            return ServiceResult<List<UserEquipmentDto>>.Failure("UserId is required");

        if (quantity < 0)
            return ServiceResult<List<UserEquipmentDto>>.Failure("Quantity can't be negative");
        
        var activity = await _friendoutDbContext.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
        
        if(activity is null)
            return ServiceResult<List<UserEquipmentDto>>.Failure("Cannot find this activity");
        
        try
        {
            var existingUserEquipment = await _friendoutDbContext.UserEquipment
                .FirstOrDefaultAsync(ue =>
                    ue.UserId == userId &&
                    ue.EquipmentId == equipmentId &&
                    ue.ActivityId == activityId);
            
            if (existingUserEquipment == null)
            {
                // Rien à supprimer
                if (quantity <= 0)
                    return await GetUserEquipmentsAsync(userId, activityId);

                var equipmentExists = await _friendoutDbContext.Equipment
                    .AnyAsync(e => e.Id == equipmentId);

                if (!equipmentExists)
                    return ServiceResult<List<UserEquipmentDto>>.Failure("Equipment not found");

                var userExists = await _friendoutDbContext.Users
                    .AnyAsync(u => u.Id == userId);

                if (!userExists)
                    return ServiceResult<List<UserEquipmentDto>>.Failure("User not found");

                _friendoutDbContext.UserEquipment.Add(new UserEquipment
                {
                    UserId = userId,
                    EquipmentId = equipmentId,
                    ActivityId = activityId,
                    Quantity = quantity
                });
            }
            else
            {
                
                if (quantity <= 0)
                {
                    _friendoutDbContext.UserEquipment.Remove(existingUserEquipment);
                }
                else
                {
                    existingUserEquipment.Quantity = quantity;
                }
            }

            await _friendoutDbContext.SaveChangesAsync();

            return await GetUserEquipmentsAsync(userId, activityId);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            // A concurrent request inserted the same row between our FirstOrDefaultAsync (null)
            // and our SaveChangesAsync. Clear the tracker and retry as an update.
            _logger.LogWarning(
                "Duplicate key race condition on UserEquipment (userId={UserId}, equipmentId={EquipmentId}, activityId={ActivityId}). Retrying as update.",
                userId, equipmentId, activityId);

            _friendoutDbContext.ChangeTracker.Clear();

            var conflictEntry = await _friendoutDbContext.UserEquipment
                .FirstOrDefaultAsync(ue =>
                    ue.UserId == userId &&
                    ue.EquipmentId == equipmentId &&
                    ue.ActivityId == activityId);

            if (conflictEntry is not null)
            {
                if (quantity <= 0)
                    _friendoutDbContext.UserEquipment.Remove(conflictEntry);
                else
                    conflictEntry.Quantity = quantity;

                await _friendoutDbContext.SaveChangesAsync();
            }

            return await GetUserEquipmentsAsync(userId, activityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to set user equipment for user {UserId} and equipment {EquipmentId}",
                userId,
                equipmentId);

            return ServiceResult<List<UserEquipmentDto>>
                .Failure("An error occurred while updating user equipment");
        }
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        return ex.InnerException is MySqlException { Number: 1062 };
    }


    public async Task<ServiceResult<bool>> UserHasEquipmentAsync(string equipmentId, string userId)
    {
        try
        {
            var hasEquipment = await _friendoutDbContext.UserEquipment
                .AnyAsync(ue => ue.UserId == userId && ue.EquipmentId == equipmentId);

            return ServiceResult<bool>.Success(hasEquipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check if user {userId} has equipment {equipmentId}");
            return ServiceResult<bool>.Failure(
                "An error occurred while checking user equipment");
        }
    }
}
