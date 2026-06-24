using System.Threading.Tasks;
using Friendout.Domain.DTOs.Preferences;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

/// <summary>
/// Manages a user's own preferences (locale + notification channels).
/// </summary>
public interface IUserPreferencesService
{
    Task<UserPreferencesDto> GetMyPreferencesAsync(string userId);
    Task<ServiceResult<UserPreferencesDto>> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesDto dto);
}
