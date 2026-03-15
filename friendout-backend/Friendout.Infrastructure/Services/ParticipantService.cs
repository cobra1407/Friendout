using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Command.Participant;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Friendout.Infrastructure.Services;

public class ParticipantService : IParticipantService
{
    private readonly FriendoutDbContext _friendoutDbContext;
    private readonly ILogger<ParticipantService> _logger;

    public ParticipantService(FriendoutDbContext friendoutDbContext, ILogger<ParticipantService> logger)
    {
        _friendoutDbContext = friendoutDbContext;
        _logger = logger;
    }

    public async Task<ServiceResult<UserActivityParticipantsDto>> GetActivityParticipantsAsync(string activityId)
    {
        if (string.IsNullOrWhiteSpace(activityId))
            return ServiceResult<UserActivityParticipantsDto>.Failure("ActivityId is required.");

        var activity = await _friendoutDbContext.Activities.FirstOrDefaultAsync(a => a.Id == activityId);

        if (activity is null)
            return ServiceResult<UserActivityParticipantsDto>.Failure("Cannot find this activity");

        try
        {
            var mainParticipants = await _friendoutDbContext.UserParticipation
                .Where(up => up.ActivityId == activityId && up.SubActivityId == null)
                .Select(up => new ParticipantDto
                {
                    ParticipationId = up.Id,
                    Username = up.User.Name,
                    AvatarUrl = up.User.AvatarUrl,
                    ParticipationStatus = up.Status
                })
                .ToListAsync();

            var subParticipants = await _friendoutDbContext.UserParticipation
                .Where(up => up.ActivityId == activityId && up.SubActivityId != null)
                .Select(up => new ParticipantDto
                {
                    ParticipationId = up.Id,
                    Username = up.User.Name,
                    AvatarUrl = up.User.AvatarUrl,
                    ParticipationStatus = up.Status,
                })
                .ToListAsync();

            var result = new UserActivityParticipantsDto
            {
                MainActivityParticipants = mainParticipants,
                SubActivityParticipants = subParticipants
            };

            return ServiceResult<UserActivityParticipantsDto>.Success(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve participants for activity {ActivityId}", activityId);
            return ServiceResult<UserActivityParticipantsDto>.Failure(
                "An error occurred while retrieving participants for the activity.");
        }
    }


   public async Task<ServiceResult<UserActivityParticipationDto>> SaveParticipationAsync(
    UpdateParticipationCommand command,
    string userId)
    {
        // Vérifie si l'activité existe
        var activity = await _friendoutDbContext.Activities
            .FirstOrDefaultAsync(a => a.Id == command.ActivityId);

        if (activity is null)
            return ServiceResult<UserActivityParticipationDto>.Failure("Can't participate to this activity"); // Activity doesn't exist

        // Prevent joining if activity already started
        if (activity.StartAt < DateTime.UtcNow)
            return ServiceResult<UserActivityParticipationDto>.Failure("This activity is already started");

        try
        {
            // Précharge toutes les participations existantes pour cet utilisateur + activité
            var existingParticipations = await _friendoutDbContext.UserParticipation
                .Where(up => up.UserId == userId && up.ActivityId == command.ActivityId)
                .ToListAsync();

            // Nettoie les subActivityIds pour enlever null et doublons
            var subActivityIds = command.SubActivityIds?
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList() ?? new List<string>();

            // ----------------
            // PARTICIPATION PRINCIPALE
            // ----------------
            if (!subActivityIds.Any())
            {
                var mainParticipation = existingParticipations
                    .FirstOrDefault(up => up.SubActivityId == null);

                if (mainParticipation != null)
                {
                    // Update
                    mainParticipation.Status = command.Status;
                    mainParticipation.UpdatedAt = DateTime.UtcNow;
                    _friendoutDbContext.UserParticipation.Update(mainParticipation);
                }
                else
                {
                    // Create
                    mainParticipation = new UserParticipation
                    {
                        UserId = userId,
                        ActivityId = command.ActivityId,
                        SubActivityId = null,
                        Status = command.Status,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _friendoutDbContext.UserParticipation.Add(mainParticipation);
                }
            }
            else
            {
                // ----------------
                // PARTICIPATION AUX SOUS-ACTIVITÉS
                // ----------------
                var validSubActivityIds = await _friendoutDbContext.SubActivities
                    .Where(sa => sa.ActivityId == command.ActivityId && subActivityIds.Contains(sa.Id))
                    .Select(sa => sa.Id)
                    .ToListAsync();

                foreach (var subActivityId in validSubActivityIds)
                {
                    var participation = existingParticipations
                        .FirstOrDefault(up => up.SubActivityId == subActivityId);

                    if (participation != null)
                    {
                        participation.Status = command.Status;
                        _friendoutDbContext.UserParticipation.Update(participation);
                    }
                    else
                    {
                        participation = new UserParticipation
                        {
                            UserId = userId,
                            ActivityId = command.ActivityId,
                            SubActivityId = subActivityId,
                            Status = command.Status,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _friendoutDbContext.UserParticipation.Add(participation);
                    }
                }
            }

            // Sauvegarde tout
            await _friendoutDbContext.SaveChangesAsync();

            // ----------------
            // Récupère toutes les participations pour cette activité
            // ----------------
            var userParticipations = await _friendoutDbContext.UserParticipation
                .Where(up =>
                    up.ActivityId == command.ActivityId &&
                    up.UserId == userId
                )
                .Include(up => up.User)
                .ToListAsync();

            var allParticipations = await _friendoutDbContext.UserParticipation
                .Where(up => up.ActivityId == command.ActivityId)
                .Include(up => up.User)
                .ToListAsync();


            var mainActivityParticipation =
                userParticipations.FirstOrDefault(p => p.SubActivityId == null);

            var subActivitiesParticipation =
                userParticipations.Where(p => p.SubActivityId != null).ToList();



            var result = new UserActivityParticipationDto
            {
                // =====================
                // USER PARTICIPATION
                // =====================
                UserMainParticipation = mainActivityParticipation != null
                    ? new UserParticipationDto
                    {
                        ActivityId = mainActivityParticipation.ActivityId,
                        SubActivityId = mainActivityParticipation.SubActivityId,
                        Status = mainActivityParticipation.Status
                    }
                    : null,

                UserSubActivitiesParticipations = subActivitiesParticipation
                    .Select(s => new UserParticipationDto
                    {
                        ActivityId = s.ActivityId,
                        SubActivityId = s.SubActivityId,
                        Status = s.Status
                    })
                    .ToList(),

                // =====================
                // ALL PARTICIPANTS
                // =====================
                MainActivityParticipants = allParticipations
                    .Where(p => p.SubActivityId == null)
                    .Select(p => new ParticipantDto
                    {
                        ParticipationId = p.Id,
                        Username = p.User.Name,
                        AvatarUrl = p.User.AvatarUrl,
                        ParticipationStatus = p.Status
                    })
                    .ToList(),

                SubActivitiesParticipants = allParticipations
                    .Where(p => p.SubActivityId != null)
                    .Select(p => new ParticipantDto
                    {
                        ParticipationId = p.Id,
                        Username = p.User.Name,
                        AvatarUrl = p.User.AvatarUrl,
                        ParticipationStatus = p.Status,
                        SubActivityId = p.SubActivityId
                    })
                    .ToList()
            };


            return ServiceResult<UserActivityParticipationDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating participation for user {UserId}", userId);
            return ServiceResult<UserActivityParticipationDto>.Failure("Error occurred when updating participation");
        }
    }
}
