using friendout_backend.RequestModels.Activity;
using friendout_backend.RequestModels.SubActivity;
using Friendout.Domain.DTOs.Activity;
using Friendout.Domain.DTOs.SubActivity;
using Friendout.Domain.Models;
using System;
using System.Linq;

namespace friendout_backend.Mappers
{
    /// <summary>
    /// Mapper to convert API RequestModels into Domain DTOs.
    /// This isolates web layer dependencies from the Domain layer.
    /// </summary>
    public static class ActivityMapper
    {
        private static string? NormalizeOptional(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private static List<string> NormalizeEquipmentNames(List<string>? values)
        {
            if (values is null || values.Count == 0)
            {
                return new List<string>();
            }

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static DateTime ResolveDateTime(DateTime baseDate, string time, DateTime fallback)
        {
            if (!TimeSpan.TryParse(time, out var parsedTime))
            {
                return fallback;
            }

            return baseDate.Date.Add(parsedTime);
        }

        private static List<CreateSubActivityDto> NormalizeSubActivities(
            DateTime activityStartAt,
            List<CreateSubActivityRequest> values)
        {
            if (values.Count == 0)
            {
                return new List<CreateSubActivityDto>();
            }

            return values
                .Where(v =>
                    !string.IsNullOrWhiteSpace(v.Name) &&
                    !string.IsNullOrWhiteSpace(v.StartTime) &&
                    !string.IsNullOrWhiteSpace(v.EndTime))
                .Select(v =>
                {
                    var startTime = ResolveDateTime(activityStartAt, v.StartTime, activityStartAt);
                    var endTime = ResolveDateTime(activityStartAt, v.EndTime!, startTime);

                    return new CreateSubActivityDto
                    {
                        Id = NormalizeOptional(v.Id),
                        Name = v.Name.Trim(),
                        StartTime = startTime,
                        EndTime = endTime,
                        Description = NormalizeOptional(v.Description),
                        Price = v.Price,
                        Address = NormalizeOptional(v.Address),
                        MapLink = NormalizeOptional(v.MapLink),
                        VirtualUrl = NormalizeOptional(v.VirtualUrl)
                    };
                })
                .ToList();
        } 

        /// <summary>
        /// Converts a <see cref="CreateActivityRequest"/> (API) into a <see cref="CreateActivityDto"/> (Domain).
        /// </summary>
        /// <param name="request">The API request model containing activity data.</param>
        /// <param name="activityImage">
        /// Optional file upload converted from IFormFile.
        /// Can be null if no image is provided.
        /// </param>
        /// <returns>A <see cref="CreateActivityDto"/> to be used in the Domain layer.</returns>
        public static CreateActivityDto ToCreateActivityDto(CreateActivityRequest request, FileUpload? activityImage)
        {
            return new CreateActivityDto
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                StartAt = request.StartAt,
                EndAt = request.EndAt ?? request.StartAt, // fallback if EndAt is null
                Time = request.Time.Trim(),

                // Localisation fields
                Address = NormalizeOptional(request.Address),
                MapLink = NormalizeOptional(request.MapLink),
                VirtualUrl = NormalizeOptional(request.VirtualUrl),

                EstimatedPrice = request.EstimatedPrice,
                RequiredEquipmentNames = NormalizeEquipmentNames(request.ResolveRequiredEquipmentNames()),
                SubActivities = NormalizeSubActivities(request.StartAt, request.ResolveSubActivities()),
                ActivityImage = activityImage
            };
        }

        public static UpdateActivityDto ToUpdateActivityDto(string activityId, CreateActivityRequest request, FileUpload? activityImage)
        {
            return new UpdateActivityDto
            {
                Id = activityId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                StartAt = request.StartAt,
                EndAt = request.EndAt ?? request.StartAt,

                Address = NormalizeOptional(request.Address),
                MapLink = NormalizeOptional(request.MapLink),
                VirtualUrl = NormalizeOptional(request.VirtualUrl),

                EstimatedPrice = request.EstimatedPrice,
                RequiredEquipmentNames = NormalizeEquipmentNames(request.ResolveRequiredEquipmentNames()),
                SubActivities = NormalizeSubActivities(request.StartAt, request.ResolveSubActivities()),
                ActivityImage = activityImage,
                RemoveImage = request.RemoveImage
            };
        }
    }
}
