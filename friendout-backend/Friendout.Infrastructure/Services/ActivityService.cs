using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.DTOs.Activity;
using Friendout.Domain.DTOs.Comment;
using Friendout.Domain.DTOs.Equipment;
using Friendout.Domain.DTOs.Image;
using Friendout.Domain.DTOs.Localisation;
using Friendout.Domain.DTOs.Participant;
using Friendout.Domain.DTOs.SubActivity;
using Friendout.Domain.Enums;
using Friendout.Domain.Enums.FilterEnums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;
using Friendout.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Friendout.Infrastructure.Services;

public class ActivityService : IActivityService
{
    private readonly FriendoutDbContext _friendoutDbContext;
    private readonly ILogger<ActivityService> _logger;
    private readonly IFileService _fileService;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AppOptions _appOptions;

    public ActivityService(
        FriendoutDbContext friendoutDbContext,
        ILogger<ActivityService> logger,
        IFileService fileService,
        INotificationDispatcher notificationDispatcher,
        IServiceScopeFactory scopeFactory,
        IOptions<AppOptions> appOptions)
    {
        _friendoutDbContext = friendoutDbContext;
        _logger = logger;
        _fileService = fileService;
        _notificationDispatcher = notificationDispatcher;
        _scopeFactory = scopeFactory;
        _appOptions = appOptions.Value;
    }

    private static string BuildLocalisationDisplayName(LocalisationType type, string? address, string? mapLink, string? virtualUrl)
    {
        return type switch
        {
            LocalisationType.Address => address ?? "Adresse",
            LocalisationType.MapLink => ExtractLocalisationNameFromMapLink(mapLink) ?? "Lieu depuis Google Maps",
            LocalisationType.Virtual => !string.IsNullOrWhiteSpace(virtualUrl) ? virtualUrl : "Lieu virtuel",
            _ => "Lieu"
        };
    }

    private static string? ExtractLocalisationNameFromMapLink(string? mapLink)
    {
        if (string.IsNullOrWhiteSpace(mapLink))
            return null;

        try
        {
            var uri = new Uri(mapLink);
            var placeMatch = Regex.Match(uri.AbsolutePath, @"/maps/place/([^/]+)");
            if (placeMatch.Success)
                return Uri.UnescapeDataString(placeMatch.Groups[1].Value.Replace("+", " "));

            string? q = null;
            var rawQuery = uri.Query.TrimStart('?');
            foreach (var pair in rawQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length != 2) continue;
                var key = parts[0];
                if (key != "q" && key != "query" && key != "destination") continue;
                q = parts[1];
                break;
            }

            if (!string.IsNullOrWhiteSpace(q))
                return Uri.UnescapeDataString(q.Replace("+", " "));
        }
        catch
        {
            // Ignore parsing error and fallback to default label.
        }

        return null;
    }

    public async Task<ServiceResult<List<ActivityDto>>> GetActivitiesAsync(string userId, ActivityFilterDto filter)
    {
        try
        {
            var query = _friendoutDbContext.Activities
                .AsNoTracking()
                .AsQueryable();

            if (filter.OnlyOwnActivity)
                query = query.Where(a => a.CreatedBy == userId);

            var now = DateTime.UtcNow;
            query = filter.TimeFilter switch
            {
                ActivityTimeFilter.Upcoming => query.Where(a => a.StartAt >= now),
                ActivityTimeFilter.Past => query.Where(a => a.StartAt < now),
                _ => query
            };

            query = query
                .OrderBy(a => a.StartAt < now)
                .ThenBy(a => a.StartAt < now ? DateTime.MaxValue : a.StartAt)
                .ThenByDescending(a => a.StartAt < now ? a.StartAt : DateTime.MinValue);

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(search) || a.Description.ToLower().Contains(search));
            }

            query = query.Skip(filter.Skip).Take(filter.Take);

            var activities = await query
                .Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    EstimatedPrice = a.EstimatedPrice,
                    HasEquipment = a.ActivityEquipments != null && a.ActivityEquipments.Any(),
                    SubActivities = a.SubActivities.Select(sa => new SubActivityDto
                    {
                        Id = sa.Id,
                        Name = sa.Name,
                        Price = sa.Price,
                        StartTime = sa.StartTime,
                        EndTime = sa.EndTime,
                        Localisation = sa.Localisation == null ? null : new LocalisationDto
                        {
                            Type = sa.Localisation.Type,
                            Address = sa.Localisation.Address,
                            MapLink = sa.Localisation.MapLink,
                            VirtualUrl = sa.Localisation.VirtualUrl,
                            DisplayName = sa.Localisation.DisplayName
                        }
                    }).ToList(),
                    NbParticipants = a.UserParticipations.Select(u => u.UserId).Distinct().Count(),
                    Localisation = new LocalisationDto
                    {
                        Type = a.Localisation.Type,
                        Address = a.Localisation.Address,
                        MapLink = a.Localisation.MapLink,
                        VirtualUrl = a.Localisation.VirtualUrl,
                        DisplayName = a.Localisation.DisplayName
                    },
                    Image = a.Image != null ? new ImageDto { Id = a.Image.Id, Url = a.Image.Url, AltText = a.Image.AltText } : null,
                    CreatedBy = a.Creator.Name,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return ServiceResult<List<ActivityDto>>.Success(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve activities\nError: {ex.Message}");
            return ServiceResult<List<ActivityDto>>.Failure("An error occurred while retrieving activities");
        }
    }

    public async Task<ServiceResult<ActivityDetailsDto>> GetActivityByIdAsync(string activityId, string userId)
    {
        try
        {
            var userEquipments = await _friendoutDbContext.UserEquipment
                .Where(ue => ue.UserId == userId && activityId == ue.ActivityId)
                .Select(eu => new UserEquipmentDto
                {
                    EquipmentId = eu.EquipmentId,
                    Quantity = eu.Quantity,
                    Description = eu.Equipment.Description,
                    Name = eu.Equipment.Name
                })
                .ToListAsync();

            var activity = await _friendoutDbContext.Activities
                .AsNoTracking()
                .Where(a => a.Id == activityId)
                .Select(a => new ActivityDetailsDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    EstimatedPrice = a.EstimatedPrice,
                    TotalPrice = (a.EstimatedPrice ?? 0) + (a.SubActivities.Any() ? a.SubActivities.Sum(sa => sa.Price ?? 0) : 0),
                    CreatedBy = a.Creator.Name,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    Image = a.Image == null ? null : new ImageDto { Id = a.Image.Id, Url = a.Image.Url, AltText = a.Image.AltText },
                    Localisation = a.Localisation == null ? null : new LocalisationDto
                    {
                        Type = a.Localisation.Type,
                        Address = a.Localisation.Address,
                        MapLink = a.Localisation.MapLink,
                        VirtualUrl = a.Localisation.VirtualUrl,
                        DisplayName = a.Localisation.DisplayName
                    },
                    UserMainParticipation = a.UserParticipations
                        .Where(up => up.UserId == userId && up.SubActivityId == null)
                        .Select(up => new UserParticipationDto { ActivityId = up.ActivityId, Status = up.Status })
                        .FirstOrDefault(),
                    Participants = a.UserParticipations
                        .Where(up => up.SubActivityId == null)
                        .Select(up => new ParticipantDto
                        {
                            ParticipationId = up.Id,
                            Username = up.User.Name,
                            ParticipationStatus = up.Status,
                            AvatarUrl = up.User.AvatarUrl
                        })
                        .ToList(),
                    RequiredEquipments = a.ActivityEquipments
                        .Select(ae => new EquipmentDto
                        {
                            EquipmentId = ae.Equipment.Id,
                            Name = ae.Equipment.Name,
                            Description = ae.Equipment.Description,
                            Quantity = ae.Quantity
                        })
                        .ToList(),
                    UserEquipments = userEquipments,
                    SubActivities = a.SubActivities
                        .Select(sa => new SubActivityDetailsDto
                        {
                            Id = sa.Id,
                            Name = sa.Name,
                            StartTime = sa.StartTime,
                            EndTime = sa.EndTime,
                            Price = sa.Price,
                            Description = sa.Description,
                            Localisation = sa.Localisation == null ? null : new LocalisationDto
                            {
                                Type = sa.Localisation.Type,
                                Address = sa.Localisation.Address,
                                MapLink = sa.Localisation.MapLink,
                                VirtualUrl = sa.Localisation.VirtualUrl,
                                DisplayName = sa.Localisation.DisplayName
                            },
                            Participants = sa.UserParticipations
                                .OrderBy(up => up.UpdatedAt)
                                .Select(up => new ParticipantDto
                                {
                                    ParticipationId = up.Id,
                                    Username = up.User.Name,
                                    ParticipationStatus = up.Status,
                                    AvatarUrl = up.User.AvatarUrl,
                                })
                                .ToList(),
                        })
                        .ToList(),
                    UserSubActivitiesParticipations = a.UserParticipations
                        .Where(up => up.UserId == userId && up.SubActivityId != null)
                        .Select(up => new UserParticipationDto
                        {
                            ActivityId = up.ActivityId,
                            SubActivityId = up.SubActivityId,
                            Status = up.Status
                        })
                        .ToList(),
                    Comments = a.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new CommentDto
                        {
                            CommentId = c.Id,
                            Content = c.Content,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            SendBy = c.User.Name,
                            UserId = c.UserId
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (activity == null)
                return ServiceResult<ActivityDetailsDto>.Failure("Activity not found");

            return ServiceResult<ActivityDetailsDto>.Success(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve activity with ID {activityId}");
            return ServiceResult<ActivityDetailsDto>.Failure("An error occurred while retrieving the activity");
        }
    }

    public async Task<ServiceResult<ActivityDto>> CreateActivityAsync(CreateActivityDto createActivityDto, string userId)
    {
        try
        {
            if (createActivityDto.StartAt == default)
                return ServiceResult<ActivityDto>.Failure("La date de debut est invalide.");
            if (createActivityDto.EndAt < createActivityDto.StartAt)
                return ServiceResult<ActivityDto>.Failure("La date de fin doit etre superieure ou egale a la date de debut.");
            if (createActivityDto.EstimatedPrice.HasValue && createActivityDto.EstimatedPrice.Value < 0)
                return ServiceResult<ActivityDto>.Failure("Le prix estime ne peut pas etre negatif.");
            if (createActivityDto.SubActivities.Any(sa => sa.Price.HasValue && sa.Price.Value < 0))
                return ServiceResult<ActivityDto>.Failure("Le prix d'une sous-activite ne peut pas etre negatif.");
            if (createActivityDto.SubActivities.Any(sa => sa.EndTime <= sa.StartTime))
                return ServiceResult<ActivityDto>.Failure("L'heure de fin d'une sous-activite doit etre strictement apres l'heure de debut.");

            var requiredEquipmentNames = createActivityDto.RequiredEquipmentNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var normalizedSubActivities = createActivityDto.SubActivities
                .Where(sa => !string.IsNullOrWhiteSpace(sa.Name))
                .Select(sa => new { Name = sa.Name.Trim(), sa.StartTime, sa.EndTime, sa.Description, sa.Price, sa.Address, sa.MapLink, sa.VirtualUrl })
                .ToList();

            string? imageId = null;
            if (createActivityDto.ActivityImage != null)
            {
                string fileName;
                try { fileName = await _fileService.SaveFileAsync(createActivityDto.ActivityImage, FileCategory.ActivityImage); }
                catch (ArgumentException ex) { return ServiceResult<ActivityDto>.Failure($"Image invalide : {ex.Message}"); }

                var fileUrl = _fileService.GetFileUrl(fileName, FileCategory.ActivityImage);
                var image = new Image
                {
                    Id = Guid.NewGuid().ToString(), Url = fileUrl,
                    Name = createActivityDto.ActivityImage.FileName,
                    MimeType = createActivityDto.ActivityImage.ContentType,
                    Size = createActivityDto.ActivityImage.Length,
                    CreatedBy = userId
                };
                _friendoutDbContext.Images.Add(image);
                imageId = image.Id;
            }

            Localisation localisation;
            if (!string.IsNullOrEmpty(createActivityDto.Address) || !string.IsNullOrEmpty(createActivityDto.MapLink) || !string.IsNullOrEmpty(createActivityDto.VirtualUrl))
            {
                var type = !string.IsNullOrEmpty(createActivityDto.Address) ? LocalisationType.Address
                    : !string.IsNullOrEmpty(createActivityDto.MapLink) ? LocalisationType.MapLink : LocalisationType.Virtual;
                localisation = new Localisation
                {
                    Id = Guid.NewGuid().ToString(), Type = type,
                    Address = createActivityDto.Address, MapLink = createActivityDto.MapLink, VirtualUrl = createActivityDto.VirtualUrl,
                    DisplayName = BuildLocalisationDisplayName(type, createActivityDto.Address, createActivityDto.MapLink, createActivityDto.VirtualUrl)
                };
                _friendoutDbContext.Localisations.Add(localisation);
            }
            else
            {
                localisation = new Localisation { Id = Guid.NewGuid().ToString(), Type = LocalisationType.Address, DisplayName = "Lieu non specifie" };
                _friendoutDbContext.Localisations.Add(localisation);
            }

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                Title = createActivityDto.Title, Description = createActivityDto.Description,
                StartAt = createActivityDto.StartAt, EndAt = createActivityDto.EndAt,
                EstimatedPrice = createActivityDto.EstimatedPrice, ImageId = imageId,
                Localisation = localisation, CreatedBy = userId,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _friendoutDbContext.Activities.Add(activity);

            if (normalizedSubActivities.Count > 0)
            {
                var subActivities = normalizedSubActivities.Select(sa =>
                {
                    var subLocalisation = localisation;
                    if (!string.IsNullOrWhiteSpace(sa.Address) || !string.IsNullOrWhiteSpace(sa.MapLink) || !string.IsNullOrWhiteSpace(sa.VirtualUrl))
                    {
                        var subType = !string.IsNullOrWhiteSpace(sa.Address) ? LocalisationType.Address
                            : !string.IsNullOrWhiteSpace(sa.MapLink) ? LocalisationType.MapLink : LocalisationType.Virtual;
                        subLocalisation = new Localisation
                        {
                            Id = Guid.NewGuid().ToString(), Type = subType,
                            Address = sa.Address, MapLink = sa.MapLink, VirtualUrl = sa.VirtualUrl,
                            DisplayName = BuildLocalisationDisplayName(subType, sa.Address, sa.MapLink, sa.VirtualUrl)
                        };
                        _friendoutDbContext.Localisations.Add(subLocalisation);
                    }
                    return new SubActivity
                    {
                        Id = Guid.NewGuid().ToString(), Name = sa.Name,
                        StartTime = sa.StartTime, EndTime = sa.EndTime,
                        Description = sa.Description, Price = sa.Price,
                        ActivityId = activity.Id, Localisation = subLocalisation,
                        LocalisationId = subLocalisation.Id,
                        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                    };
                }).ToList();
                _friendoutDbContext.SubActivities.AddRange(subActivities);
            }

            if (requiredEquipmentNames.Count > 0)
            {
                var normalizedRequiredNames = requiredEquipmentNames.Select(name => name.ToLowerInvariant()).ToHashSet();
                var existingEquipments = await _friendoutDbContext.Equipment.Where(e => normalizedRequiredNames.Contains(e.Name.ToLower())).ToListAsync();
                var existingNormalizedNames = existingEquipments.Select(e => e.Name.ToLowerInvariant()).ToHashSet();
                var newEquipments = requiredEquipmentNames.Where(name => !existingNormalizedNames.Contains(name.ToLowerInvariant()))
                    .Select(name => new Equipment { Id = Guid.NewGuid().ToString(), Name = name }).ToList();
                if (newEquipments.Count > 0) _friendoutDbContext.Equipment.AddRange(newEquipments);
                var allEquipments = existingEquipments.Concat(newEquipments).GroupBy(e => e.Id).Select(g => g.First()).ToList();
                _friendoutDbContext.ActivityEquipment.AddRange(allEquipments.Select(e => new ActivityEquipment
                {
                    Id = Guid.NewGuid().ToString(), ActivityId = activity.Id, EquipmentId = e.Id,
                    Required = true, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                }));
            }

            await _friendoutDbContext.SaveChangesAsync();

            var updatedActivityDto = await _friendoutDbContext.Activities.AsNoTracking().Where(a => a.Id == activity.Id)
                .Select(a => new ActivityDto
                {
                    Id = a.Id, Title = a.Title, Description = a.Description,
                    StartAt = a.StartAt, EndAt = a.EndAt, EstimatedPrice = a.EstimatedPrice,
                    CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, CreatedBy = a.Creator.Name,
                    SubActivities = a.SubActivities.Select(sa => new SubActivityDto
                    {
                        Id = sa.Id, Name = sa.Name, StartTime = sa.StartTime, EndTime = sa.EndTime, Price = sa.Price,
                        Localisation = sa.Localisation == null ? null : new LocalisationDto
                        {
                            Type = sa.Localisation.Type, Address = sa.Localisation.Address,
                            MapLink = sa.Localisation.MapLink, VirtualUrl = sa.Localisation.VirtualUrl,
                            DisplayName = sa.Localisation.DisplayName
                        }
                    }).ToList(),
                    HasEquipment = a.ActivityEquipments != null && a.ActivityEquipments.Any(),
                    Localisation = new LocalisationDto
                    {
                        Type = a.Localisation.Type, Address = a.Localisation.Address,
                        MapLink = a.Localisation.MapLink, VirtualUrl = a.Localisation.VirtualUrl,
                        DisplayName = a.Localisation.DisplayName
                    },
                    Image = a.Image == null ? null : new ImageDto { Id = a.Image.Id, Url = a.Image.Url, AltText = a.Image.Name }
                }).FirstAsync();

            return ServiceResult<ActivityDto>.Success(updatedActivityDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create activity");
            return ServiceResult<ActivityDto>.Failure("An error occurred while creating the activity");
        }
    }

    public async Task<ServiceResult<ActivityDto>> UpdateActivityAsync(UpdateActivityDto activityDto, string userId)
    {
        try
        {
            if (activityDto.StartAt == default) return ServiceResult<ActivityDto>.Failure("La date de debut est invalide.");
            if (activityDto.EndAt < activityDto.StartAt) return ServiceResult<ActivityDto>.Failure("La date de fin doit etre superieure ou egale a la date de debut.");
            if (activityDto.EstimatedPrice.HasValue && activityDto.EstimatedPrice.Value < 0) return ServiceResult<ActivityDto>.Failure("Le prix estime ne peut pas etre negatif.");
            if (activityDto.SubActivities.Any(sa => sa.Price.HasValue && sa.Price.Value < 0)) return ServiceResult<ActivityDto>.Failure("Le prix d'une sous-activite ne peut pas etre negatif.");
            if (activityDto.SubActivities.Any(sa => sa.EndTime <= sa.StartTime)) return ServiceResult<ActivityDto>.Failure("L'heure de fin d'une sous-activite doit etre strictement apres l'heure de debut.");

            var activity = await _friendoutDbContext.Activities
                .Include(a => a.Localisation).Include(a => a.Image)
                .Include(a => a.SubActivities).Include(a => a.ActivityEquipments)
                .Include(a => a.Creator)
                .FirstOrDefaultAsync(a => a.Id == activityDto.Id);

            if (activity is null) return ServiceResult<ActivityDto>.Failure("Activity not found.");
            if (!string.Equals(activity.CreatedBy, userId, StringComparison.Ordinal)) return ServiceResult<ActivityDto>.Failure("You are not allowed to update this activity.");

            var requiredEquipmentNames = activityDto.RequiredEquipmentNames
                .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var normalizedSubActivities = activityDto.SubActivities
                .Where(sa => !string.IsNullOrWhiteSpace(sa.Name))
                .Select(sa => new
                {
                    Id = string.IsNullOrWhiteSpace(sa.Id) ? null : sa.Id.Trim(),
                    Name = sa.Name.Trim(), sa.StartTime, sa.EndTime, sa.Description, sa.Price, sa.Address, sa.MapLink, sa.VirtualUrl
                }).ToList();

            if (activityDto.RemoveImage && activity.Image != null)
            {
                var removeParts = activity.Image.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var removeFileName = removeParts.Length > 0 ? removeParts[^1] : null;
                if (!string.IsNullOrWhiteSpace(removeFileName))
                    try { await _fileService.DeleteFileAsync(removeFileName, FileCategory.ActivityImage); } catch { }
                _friendoutDbContext.Images.Remove(activity.Image);
                activity.ImageId = null;
                activity.Image = null;
            }
            else if (activityDto.ActivityImage != null)
            {
                string? previousFileName = null;
                if (activity.Image?.Url is { Length: > 0 } previousUrl)
                {
                    var parts = previousUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    previousFileName = parts.Length > 0 ? parts[^1] : null;
                }
                string newFileName;
                try { newFileName = await _fileService.SaveFileAsync(activityDto.ActivityImage, FileCategory.ActivityImage); }
                catch (ArgumentException ex) { return ServiceResult<ActivityDto>.Failure($"Image invalide : {ex.Message}"); }
                var fileUrl = _fileService.GetFileUrl(newFileName, FileCategory.ActivityImage);
                if (!string.IsNullOrWhiteSpace(previousFileName))
                    try { await _fileService.DeleteFileAsync(previousFileName, FileCategory.ActivityImage); } catch { }
                if (activity.Image is null)
                {
                    var image = new Image { Id = Guid.NewGuid().ToString(), Url = fileUrl, Name = activityDto.ActivityImage.FileName, MimeType = activityDto.ActivityImage.ContentType, Size = activityDto.ActivityImage.Length, CreatedBy = userId };
                    _friendoutDbContext.Images.Add(image);
                    activity.ImageId = image.Id;
                }
                else
                {
                    activity.Image.Url = fileUrl;
                    activity.Image.Name = activityDto.ActivityImage.FileName;
                    activity.Image.MimeType = activityDto.ActivityImage.ContentType;
                    activity.Image.Size = activityDto.ActivityImage.Length;
                }
            }

            Localisation localisation;
            if (!string.IsNullOrWhiteSpace(activityDto.Address) || !string.IsNullOrWhiteSpace(activityDto.MapLink) || !string.IsNullOrWhiteSpace(activityDto.VirtualUrl))
            {
                var type = !string.IsNullOrWhiteSpace(activityDto.Address) ? LocalisationType.Address
                    : !string.IsNullOrWhiteSpace(activityDto.MapLink) ? LocalisationType.MapLink : LocalisationType.Virtual;
                localisation = activity.Localisation;
                localisation.Type = type;
                localisation.Address = activityDto.Address;
                localisation.MapLink = activityDto.MapLink;
                localisation.VirtualUrl = activityDto.VirtualUrl;
                localisation.DisplayName = BuildLocalisationDisplayName(type, activityDto.Address, activityDto.MapLink, activityDto.VirtualUrl);
            }
            else
            {
                localisation = activity.Localisation ?? new Localisation { Id = Guid.NewGuid().ToString() };
                localisation.Type = LocalisationType.Address;
                localisation.Address = null; localisation.MapLink = null; localisation.VirtualUrl = null;
                localisation.DisplayName = "Lieu non specifie";
                if (activity.Localisation is null) _friendoutDbContext.Localisations.Add(localisation);
            }

            activity.Title = activityDto.Title;
            activity.Description = activityDto.Description;
            activity.StartAt = activityDto.StartAt;
            activity.EndAt = activityDto.EndAt;
            activity.EstimatedPrice = activityDto.EstimatedPrice;
            activity.Localisation = localisation;
            activity.UpdatedAt = DateTime.UtcNow;

            var existingSubActivities = await _friendoutDbContext.SubActivities.Include(sa => sa.Localisation).Where(sa => sa.ActivityId == activity.Id).ToListAsync();
            var existingSubActivitiesById = existingSubActivities.ToDictionary(sa => sa.Id, sa => sa);
            var keptSubActivityIds = new HashSet<string>(StringComparer.Ordinal);

            if (normalizedSubActivities.Count > 0)
            {
                var newSubActivities = normalizedSubActivities.Select(sa =>
                {
                    var hasOwnLocalisation = !string.IsNullOrWhiteSpace(sa.Address) || !string.IsNullOrWhiteSpace(sa.MapLink) || !string.IsNullOrWhiteSpace(sa.VirtualUrl);
                    if (!string.IsNullOrWhiteSpace(sa.Id) && existingSubActivitiesById.TryGetValue(sa.Id, out var existingSubActivity))
                    {
                        existingSubActivity.Name = sa.Name; existingSubActivity.StartTime = sa.StartTime;
                        existingSubActivity.EndTime = sa.EndTime; existingSubActivity.Description = sa.Description;
                        existingSubActivity.Price = sa.Price; existingSubActivity.UpdatedAt = DateTime.UtcNow;
                        if (hasOwnLocalisation)
                        {
                            var subType = !string.IsNullOrWhiteSpace(sa.Address) ? LocalisationType.Address
                                : !string.IsNullOrWhiteSpace(sa.MapLink) ? LocalisationType.MapLink : LocalisationType.Virtual;
                            if (existingSubActivity.Localisation is null || existingSubActivity.LocalisationId == localisation.Id)
                            {
                                var ownLoc = new Localisation { Id = Guid.NewGuid().ToString(), Type = subType, Address = sa.Address, MapLink = sa.MapLink, VirtualUrl = sa.VirtualUrl, DisplayName = BuildLocalisationDisplayName(subType, sa.Address, sa.MapLink, sa.VirtualUrl) };
                                _friendoutDbContext.Localisations.Add(ownLoc);
                                existingSubActivity.Localisation = ownLoc;
                                existingSubActivity.LocalisationId = ownLoc.Id;
                            }
                            else
                            {
                                existingSubActivity.Localisation.Type = subType;
                                existingSubActivity.Localisation.Address = sa.Address;
                                existingSubActivity.Localisation.MapLink = sa.MapLink;
                                existingSubActivity.Localisation.VirtualUrl = sa.VirtualUrl;
                                existingSubActivity.Localisation.DisplayName = BuildLocalisationDisplayName(subType, sa.Address, sa.MapLink, sa.VirtualUrl);
                            }
                        }
                        else { existingSubActivity.Localisation = localisation; existingSubActivity.LocalisationId = localisation.Id; }
                        keptSubActivityIds.Add(existingSubActivity.Id);
                        return null;
                    }
                    var subLoc = localisation;
                    if (hasOwnLocalisation)
                    {
                        var subType = !string.IsNullOrWhiteSpace(sa.Address) ? LocalisationType.Address
                            : !string.IsNullOrWhiteSpace(sa.MapLink) ? LocalisationType.MapLink : LocalisationType.Virtual;
                        subLoc = new Localisation { Id = Guid.NewGuid().ToString(), Type = subType, Address = sa.Address, MapLink = sa.MapLink, VirtualUrl = sa.VirtualUrl, DisplayName = BuildLocalisationDisplayName(subType, sa.Address, sa.MapLink, sa.VirtualUrl) };
                        _friendoutDbContext.Localisations.Add(subLoc);
                    }
                    var newSub = new SubActivity { Id = Guid.NewGuid().ToString(), Name = sa.Name, StartTime = sa.StartTime, EndTime = sa.EndTime, Description = sa.Description, Price = sa.Price, ActivityId = activity.Id, Localisation = subLoc, LocalisationId = subLoc.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                    keptSubActivityIds.Add(newSub.Id);
                    return newSub;
                }).Where(sa => sa is not null).ToList();
                if (newSubActivities.Count > 0) _friendoutDbContext.SubActivities.AddRange(newSubActivities!);
            }

            var deletedSubActivities = existingSubActivities.Where(sa => !keptSubActivityIds.Contains(sa.Id)).ToList();
            if (deletedSubActivities.Count > 0)
            {
                var deletedIds = deletedSubActivities.Select(sa => sa.Id).ToList();
                var orphans = await _friendoutDbContext.UserParticipation
                    .Where(up => up.ActivityId == activity.Id && up.SubActivityId != null && deletedIds.Contains(up.SubActivityId))
                    .ToListAsync();
                if (orphans.Count > 0) _friendoutDbContext.UserParticipation.RemoveRange(orphans);
                _friendoutDbContext.SubActivities.RemoveRange(deletedSubActivities);
            }

            var existingActivityEquipments = await _friendoutDbContext.ActivityEquipment.Where(ae => ae.ActivityId == activity.Id).ToListAsync();
            if (existingActivityEquipments.Count > 0) _friendoutDbContext.ActivityEquipment.RemoveRange(existingActivityEquipments);

            if (requiredEquipmentNames.Count > 0)
            {
                var normalizedRequiredNames = requiredEquipmentNames.Select(name => name.ToLowerInvariant()).ToHashSet();
                var existingEquipments = await _friendoutDbContext.Equipment.Where(e => normalizedRequiredNames.Contains(e.Name.ToLower())).ToListAsync();
                var existingNormalizedNames = existingEquipments.Select(e => e.Name.ToLowerInvariant()).ToHashSet();
                var newEquipments = requiredEquipmentNames.Where(name => !existingNormalizedNames.Contains(name.ToLowerInvariant())).Select(name => new Equipment { Id = Guid.NewGuid().ToString(), Name = name }).ToList();
                if (newEquipments.Count > 0) _friendoutDbContext.Equipment.AddRange(newEquipments);
                var allEquipments = existingEquipments.Concat(newEquipments).GroupBy(e => e.Id).Select(g => g.First()).ToList();
                _friendoutDbContext.ActivityEquipment.AddRange(allEquipments.Select(e => new ActivityEquipment { Id = Guid.NewGuid().ToString(), ActivityId = activity.Id, EquipmentId = e.Id, Required = true, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }));
            }

            await _friendoutDbContext.SaveChangesAsync();

            // Notify all participating users that the activity has been modified.
            // Fire-and-forget — notification failure must never block the update response.
            _ = NotifyParticipantsAsync(
                activityId: activity.Id,
                excludeUserId: userId,
                type: NotificationType.ActivityModified,
                buildData: (participantId, participantName) => new Dictionary<string, string>
                {
                    { "UserName",          participantName },
                    { "ActivityName",      activity.Title },
                    { "Date",              activity.StartAt.ToString("f") },
                    { "Location",          activity.Localisation?.DisplayName ?? "" },
                    { "OrganizerName",     activity.Creator?.Name ?? "" },
                    { "AppUrl",            _appOptions.Url },
                    { "ActivityId",        activity.Id },
                    { "ActivityImageUrl",  activity.Image?.Url ?? $"{_appOptions.Url}/email-assets/default-activity-card.png" }
                }
            );

            var updatedActivityDto = await _friendoutDbContext.Activities.AsNoTracking().Where(a => a.Id == activity.Id)
                .Select(a => new ActivityDto
                {
                    Id = a.Id, Title = a.Title, Description = a.Description,
                    StartAt = a.StartAt, EndAt = a.EndAt, EstimatedPrice = a.EstimatedPrice,
                    CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, CreatedBy = a.Creator.Name,
                    SubActivities = a.SubActivities.Select(sa => new SubActivityDto
                    {
                        Id = sa.Id, Name = sa.Name, StartTime = sa.StartTime, EndTime = sa.EndTime, Price = sa.Price,
                        Localisation = new LocalisationDto { Type = sa.Localisation.Type, Address = sa.Localisation.Address, MapLink = sa.Localisation.MapLink, VirtualUrl = sa.Localisation.VirtualUrl, DisplayName = sa.Localisation.DisplayName }
                    }).ToList(),
                    HasEquipment = a.ActivityEquipments != null && a.ActivityEquipments.Any(),
                    Localisation = new LocalisationDto { Type = a.Localisation.Type, Address = a.Localisation.Address, MapLink = a.Localisation.MapLink, VirtualUrl = a.Localisation.VirtualUrl, DisplayName = a.Localisation.DisplayName },
                    Image = a.Image == null ? null : new ImageDto { Id = a.Image.Id, Url = a.Image.Url, AltText = a.Image.Name }
                }).FirstAsync();

            return ServiceResult<ActivityDto>.Success(updatedActivityDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update activity");
            return ServiceResult<ActivityDto>.Failure("An error occurred while updating the activity");
        }
    }

    public async Task<ServiceResult<ActivityDto>> DeleteActivityAsync(string activityId, string userId)
    {
        var activity = await _friendoutDbContext.Activities
            .Include(a => a.SubActivities).Include(a => a.Creator).Include(a => a.Localisation)
            .FirstOrDefaultAsync(a => a.Id == activityId && a.Creator.Id == userId);

        if (activity == null)
            return ServiceResult<ActivityDto>.Failure("Activity not found");

        var activityDto = new ActivityDto
        {
            Id = activity.Id, Title = activity.Title, Description = activity.Description,
            StartAt = activity.StartAt, EndAt = activity.EndAt,
            CreatedBy = activity.Creator.Id, CreatedAt = activity.CreatedAt, UpdatedAt = activity.UpdatedAt,
            SubActivities = activity.SubActivities?.Select(sa => new SubActivityDto { Id = sa.Id, Name = sa.Name, StartTime = sa.StartTime, EndTime = sa.EndTime }).ToList()
        };

        _ = NotifyParticipantsAsync(
            activityId: activity.Id,
            excludeUserId: userId,
            type: NotificationType.ActivityCanceled,
            buildData: (participantId, participantName) => new Dictionary<string, string>
            {
                { "UserName",         participantName },
                { "ActivityName",     activity.Title },
                { "Date",             activity.StartAt.ToString("f") },
                { "Location",         activity.Localisation?.DisplayName ?? "" },
                { "OrganizerName",    activity.Creator.Name },
                { "CancelReason",     "N/A" },
                { "AppUrl",           _appOptions.Url },
                { "ActivityImageUrl", activity.Image?.Url ?? $"{_appOptions.Url}/email-assets/default-activity-card.png" }
            }
        );

        _friendoutDbContext.Activities.Remove(activity);
        await _friendoutDbContext.SaveChangesAsync();

        return ServiceResult<ActivityDto>.Success(activityDto);
    }

    private async Task NotifyParticipantsAsync(
        string activityId,
        string excludeUserId,
        NotificationType type,
        Func<string, string, Dictionary<string, string>> buildData)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<FriendoutDbContext>();

            var participants = await db.UserParticipation
                .AsNoTracking()
                .Where(up =>
                    up.ActivityId == activityId &&
                    up.UserId != excludeUserId &&
                    up.SubActivityId == null &&
                    up.Status == ParticipationStatus.Participating)
                .Select(up => new { up.UserId, up.User.Name })
                .Distinct()
                .ToListAsync();

            var tasks = participants.Select(p =>
                _notificationDispatcher.DispatchNotificationAsync(
                    Guid.Parse(p.UserId),
                    type,
                    buildData(p.UserId, p.Name)
                )
            );

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify participants for activity {ActivityId}", activityId);
        }
    }
}
