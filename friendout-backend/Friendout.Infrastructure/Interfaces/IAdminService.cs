using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.Admin;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface IAdminService
{
    // Access Mode
    Task<AccessModeDto> GetAccessModeAsync();

    // Logs
    Task<List<AppLogDto>> GetLogsAsync(string? level, int limit);
    Task ClearLogsAsync();

    // Guilds
    Task<List<GuildDto>> GetAllowedGuildsAsync();
    Task<ServiceResult<GuildDto>> AddAllowedGuildAsync(AddGuildDto dto);
    Task<ServiceResult<bool>> DeleteAllowedGuildAsync(int id);

    // Emails
    Task<List<EmailDto>> GetAllowedEmailsAsync();
    Task<ServiceResult<EmailDto>> AddAllowedEmailAsync(AddEmailDto dto);
    Task<ServiceResult<bool>> DeleteAllowedEmailAsync(int id);

    // Access Requests
    Task<List<AccessRequestDto>> GetAccessRequestsAsync(string? status);
    Task<ServiceResult<AccessRequestDto>> ResolveAccessRequestAsync(int id, ResolveAccessRequestDto dto);

    // Users
    Task<List<UserAdminDto>> GetUsersAsync();
    Task<ServiceResult<UserAdminDto>> UpdateUserRoleAsync(string id, UpdateUserRoleDto dto);
}
