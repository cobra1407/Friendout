using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using friendout_backend.RequestModels.SubActivity;

namespace friendout_backend.RequestModels.Activity
{
    /// <summary>
    /// Request model for creating an activity in the API layer.
    /// This model contains ASP.NET Core-specific dependencies such as IFormFile.
    /// </summary>
public class CreateActivityRequest : IValidatableObject
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

        /// <summary>
        /// Title of the activity.
        /// </summary>
        [Required, MaxLength(191)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Description of the activity.
        /// </summary>
        [Required]
        public string Description { get; set; } = null!;

        /// <summary>
        /// Start date and time of the activity.
        /// </summary>
        [Required]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Optional end date and time of the activity.
        /// </summary>
        public DateTime? EndAt { get; set; }

        /// <summary>
        /// Time of the activity (stored as a string).
        /// </summary>
        [Required, MaxLength(191)]
        public string Time { get; set; } = null!;

        #region Localisation

        /// <summary>
        /// Physical address of the activity (optional).
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Map link for the activity localisation (optional).
        /// </summary>
        [MaxLength(500)]
        public string? MapLink { get; set; }

        /// <summary>
        /// Virtual URL for online activities (optional).
        /// </summary>
        [MaxLength(500)]
        public string? VirtualUrl { get; set; }

        #endregion

        /// <summary>
        /// Estimated price of the activity (optional).
        /// </summary>
        public double? EstimatedPrice { get; set; }

        /// <summary>
        /// Optional list of required equipment names.
        /// </summary>
        public List<string>? RequiredEquipmentNames { get; set; }

        /// <summary>
        /// Optional JSON fallback for required equipment names.
        /// </summary>
        public string? RequiredEquipmentNamesJson { get; set; }

        /// <summary>
        /// Optional JSON payload for sub-activities.
        /// </summary>
        public string? SubActivitiesJson { get; set; }

        /// <summary>
        /// Image file of the activity (optional). 
        /// If not provided, a default image can be used.
        /// </summary>
        public IFormFile? ActivityImage { get; set; }

        public List<string> ResolveRequiredEquipmentNames()
        {
            if (RequiredEquipmentNames is { Count: > 0 })
            {
                return RequiredEquipmentNames;
            }

            if (string.IsNullOrWhiteSpace(RequiredEquipmentNamesJson))
            {
                return new List<string>();
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(RequiredEquipmentNamesJson, JsonOptions);
                return parsed ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public List<CreateSubActivityRequest> ResolveSubActivities()
        {
            if (string.IsNullOrWhiteSpace(SubActivitiesJson))
            {
                return new List<CreateSubActivityRequest>();
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<CreateSubActivityRequest>>(SubActivitiesJson, JsonOptions);
                return parsed ?? new List<CreateSubActivityRequest>();
            }
            catch
            {
                return new List<CreateSubActivityRequest>();
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt.HasValue && EndAt.Value < StartAt)
            {
                yield return new ValidationResult(
                    "EndAt must be greater than or equal to StartAt.",
                    new[] { nameof(EndAt), nameof(StartAt) });
            }

            if (RequiredEquipmentNames is not null)
            {
                var hasInvalidName = RequiredEquipmentNames.Any(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length > 191);
                if (hasInvalidName)
                {
                    yield return new ValidationResult(
                        "Equipment names must not exceed 191 characters.",
                        new[] { nameof(RequiredEquipmentNames) });
                }
            }

            if ((RequiredEquipmentNames is null || RequiredEquipmentNames.Count == 0) &&
                !string.IsNullOrWhiteSpace(RequiredEquipmentNamesJson))
            {
                List<string>? parsed = null;
                var hasInvalidJson = false;
                try
                {
                    parsed = JsonSerializer.Deserialize<List<string>>(RequiredEquipmentNamesJson, JsonOptions);
                }
                catch
                {
                    hasInvalidJson = true;
                }

                if (hasInvalidJson)
                {
                    yield return new ValidationResult(
                        "RequiredEquipmentNamesJson must be a valid JSON array of strings.",
                        new[] { nameof(RequiredEquipmentNamesJson) });
                }

                var hasInvalidParsedName = parsed is not null &&
                                           parsed.Any(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length > 191);
                if (hasInvalidParsedName)
                {
                    yield return new ValidationResult(
                        "Equipment names must not exceed 191 characters.",
                        new[] { nameof(RequiredEquipmentNamesJson) });
                }
            }

            if (!string.IsNullOrWhiteSpace(SubActivitiesJson))
            {
                List<CreateSubActivityRequest>? parsedSubActivities = null;
                var hasInvalidSubActivitiesJson = false;
                try
                {
                    parsedSubActivities = JsonSerializer.Deserialize<List<CreateSubActivityRequest>>(SubActivitiesJson, JsonOptions);
                }
                catch
                {
                    hasInvalidSubActivitiesJson = true;
                }

                if (hasInvalidSubActivitiesJson)
                {
                    yield return new ValidationResult(
                        "SubActivitiesJson must be a valid JSON array.",
                        new[] { nameof(SubActivitiesJson) });
                }

                if (parsedSubActivities is not null)
                {
                    foreach (var subActivity in parsedSubActivities)
                    {
                        if (string.IsNullOrWhiteSpace(subActivity.Name) || subActivity.Name.Trim().Length > 191)
                        {
                            yield return new ValidationResult(
                                "Each sub-activity must have a name between 1 and 191 characters.",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(subActivity.StartTime) ||
                            !TimeSpan.TryParse(subActivity.StartTime, out _))
                        {
                            yield return new ValidationResult(
                                "Each sub-activity must have a valid StartTime in HH:mm format.",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(subActivity.EndTime) ||
                            !TimeSpan.TryParse(subActivity.EndTime, out var endTime))
                        {
                            yield return new ValidationResult(
                                "Each sub-activity must have a valid EndTime in HH:mm format.",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }

                        var startTime = TimeSpan.Parse(subActivity.StartTime);
                        if (endTime <= startTime)
                        {
                            yield return new ValidationResult(
                                "Each sub-activity EndTime must be strictly after StartTime.",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }

                        if (subActivity.Price.HasValue && subActivity.Price.Value < 0)
                        {
                            yield return new ValidationResult(
                                "Sub-activity price cannot be negative.",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }

                        var localisationFields = new[]
                        {
                            subActivity.Address?.Trim(),
                            subActivity.MapLink?.Trim(),
                            subActivity.VirtualUrl?.Trim()
                        };

                        if (localisationFields.Any(value => !string.IsNullOrWhiteSpace(value) && value!.Length > 500))
                        {
                            yield return new ValidationResult(
                                "Sub-activity localisation fields must not exceed 500 characters.",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }

                        var localisationFieldsCount = localisationFields.Count(value => !string.IsNullOrWhiteSpace(value));
                        if (localisationFieldsCount > 1)
                        {
                            yield return new ValidationResult(
                                "Sub-activity must define only one localisation type (address, mapLink or virtualUrl).",
                                new[] { nameof(SubActivitiesJson) });
                            break;
                        }
                    }
                }
            }
        }
    }
}
