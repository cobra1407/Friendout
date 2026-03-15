using System.Collections.Generic;
using System.Threading.Tasks;
using Friendout.Domain.DTOs;
using Friendout.Domain.DTOs.Activity;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface IActivityService
{
    /// <summary>
    /// Gets list of activities
    /// </summary>
    /// <returns>Activities data.</returns>
    public Task<ServiceResult<List<ActivityDto>>> GetActivitiesAsync(string userId, ActivityFilterDto filterDto);

    /// <summary>
    /// Get details of an activity by id
    /// </summary>
    /// <param name="activityid"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ServiceResult<ActivityDetailsDto>> GetActivityByIdAsync(string activityid, string userId);
    
    /// <summary>
    /// Create an activity 
    /// </summary>
    /// <param name="activityDto"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ServiceResult<ActivityDto>> CreateActivityAsync(CreateActivityDto activityDto, string userId);
    
    
    /// <summary>
    /// Update an activity 
    /// </summary>
    /// <param name="activityDto"></param>
    /// /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ServiceResult<ActivityDto>> UpdateActivityAync(UpdateActivityDto activityDto, string userId);

    /// <summary>
    /// Delete an activity 
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ServiceResult<ActivityDto>> DeleteActivityAsync(string activityId, string userId);
}