using System.Threading.Tasks;
using Friendout.Domain.DTOs.Admin;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface ISettingsService
{
    Task<AccessSettingsDto> GetAccessSettingsAsync();
    Task<ServiceResult<AccessSettingsDto>> UpdateAccessSettingsAsync(UpdateAccessSettingsDto dto);
}
