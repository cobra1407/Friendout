using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.EquipmentList;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Service for managing user-owned reusable equipment lists.
/// </summary>
public class EquipmentListService : IEquipmentListService
{
    private readonly FriendoutDbContext _friendoutDbContext;
    private readonly ILogger<EquipmentListService> _logger;

    public EquipmentListService(FriendoutDbContext context, ILogger<EquipmentListService> logger)
    {
        _friendoutDbContext = context;
        _logger = logger;
    }

    public async Task<ServiceResult<List<EquipmentListDto>>> GetUserEquipmentListsAsync(string userId)
    {
        try
        {
            var lists = await _friendoutDbContext.EquipmentLists
                .Include(l => l.Items)
                .Where(l => l.UserId == userId)
                .OrderBy(l => l.createdAt)
                .ToListAsync();

            return ServiceResult<List<EquipmentListDto>>.Success(lists.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve equipment lists for user {UserId}", userId);
            return ServiceResult<List<EquipmentListDto>>.Failure("An error occurred while retrieving equipment lists");
        }
    }

    public async Task<ServiceResult<EquipmentListDto>> GetEquipmentListByIdAsync(string equipmentListId, string userId)
    {
        try
        {
            var list = await _friendoutDbContext.EquipmentLists
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == equipmentListId && l.UserId == userId);

            if (list is null)
                return ServiceResult<EquipmentListDto>.Failure("Equipment list not found");

            return ServiceResult<EquipmentListDto>.Success(MapToDto(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve equipment list {EquipmentListId}", equipmentListId);
            return ServiceResult<EquipmentListDto>.Failure("An error occurred while retrieving the equipment list");
        }
    }

    public async Task<ServiceResult<EquipmentListDto>> CreateEquipmentListAsync(string userId, CreateEquipmentListDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ServiceResult<EquipmentListDto>.Failure("Name is required");

        var name = request.Name.Trim();

        var nameTaken = await _friendoutDbContext.EquipmentLists
            .AnyAsync(l => l.UserId == userId && l.Name == name);

        if (nameTaken)
            return ServiceResult<EquipmentListDto>.Failure("You already have an equipment list with this name");

        try
        {
            var list = new EquipmentList
            {
                UserId = userId,
                Name = name,
                Items = SanitizeItems(request.Items)
                    .Select(itemName => new EquipmentListItem { Name = itemName })
                    .ToList()
            };

            _friendoutDbContext.EquipmentLists.Add(list);
            await _friendoutDbContext.SaveChangesAsync();

            return ServiceResult<EquipmentListDto>.Success(MapToDto(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create equipment list for user {UserId}", userId);
            return ServiceResult<EquipmentListDto>.Failure("An error occurred while creating the equipment list");
        }
    }

    public async Task<ServiceResult<EquipmentListDto>> UpdateEquipmentListAsync(string equipmentListId, string userId, UpdateEquipmentListDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ServiceResult<EquipmentListDto>.Failure("Name is required");

        var name = request.Name.Trim();

        try
        {
            var list = await _friendoutDbContext.EquipmentLists
                .Include(l => l.Items)
                .FirstOrDefaultAsync(l => l.Id == equipmentListId && l.UserId == userId);

            if (list is null)
                return ServiceResult<EquipmentListDto>.Failure("Equipment list not found");

            var nameTaken = await _friendoutDbContext.EquipmentLists
                .AnyAsync(l => l.UserId == userId && l.Name == name && l.Id != equipmentListId);

            if (nameTaken)
                return ServiceResult<EquipmentListDto>.Failure("You already have an equipment list with this name");

            list.Name = name;
            list.UpdatedAt = DateTime.UtcNow;

            // Replace items entirely rather than diffing — lists are small and this keeps the logic simple.
            _friendoutDbContext.EquipmentListItems.RemoveRange(list.Items);
            list.Items = SanitizeItems(request.Items)
                .Select(itemName => new EquipmentListItem { Name = itemName, EquipmentListId = list.Id })
                .ToList();

            await _friendoutDbContext.SaveChangesAsync();

            return ServiceResult<EquipmentListDto>.Success(MapToDto(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update equipment list {EquipmentListId}", equipmentListId);
            return ServiceResult<EquipmentListDto>.Failure("An error occurred while updating the equipment list");
        }
    }

    public async Task<ServiceResult<bool>> DeleteEquipmentListAsync(string equipmentListId, string userId)
    {
        try
        {
            var list = await _friendoutDbContext.EquipmentLists
                .FirstOrDefaultAsync(l => l.Id == equipmentListId && l.UserId == userId);

            if (list is null)
                return ServiceResult<bool>.Failure("Equipment list not found");

            _friendoutDbContext.EquipmentLists.Remove(list);
            await _friendoutDbContext.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete equipment list {EquipmentListId}", equipmentListId);
            return ServiceResult<bool>.Failure("An error occurred while deleting the equipment list");
        }
    }

    /// <summary>
    /// Trims, drops blanks, and deduplicates (case-insensitive) a raw list of equipment names.
    /// </summary>
    private static List<string> SanitizeItems(List<string> rawItems)
    {
        return rawItems
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrEmpty(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static EquipmentListDto MapToDto(EquipmentList list)
    {
        return new EquipmentListDto
        {
            Id = list.Id,
            Name = list.Name,
            Items = list.Items.Select(i => i.Name).ToList(),
            CreatedAt = list.CreatedAt,
            UpdatedAt = list.UpdatedAt
        };
    }
}
