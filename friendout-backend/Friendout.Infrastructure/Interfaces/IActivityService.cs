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
    public Task<ServiceResult<ActivityDto>> UpdateActivityAsync(UpdateActivityDto activityDto, string userId);

    /// <summary>
    /// Delete an activity 
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ServiceResult<ActivityDto>> DeleteActivityAsync(string activityId, string userId);

    /// <summary>
    /// Returns the activity's public share link, generating one on first use.
    /// Idempotent: once a token exists it's reused, so a link already handed out
    /// keeps working no matter who else clicks "Share" afterwards.
    /// Any participant (or the creator) can call this — anyone who can already see
    /// the activity can share it.
    /// </summary>
    public Task<ServiceResult<ShareLinkDto>> GetOrCreateShareLinkAsync(string activityId, string userId);

    /// <summary>
    /// Returns the read-only public view of an activity for the given share token.
    /// No authentication required — used by the anonymous /share/{token} page.
    /// </summary>
    public Task<ServiceResult<PublicActivityDto>> GetPublicActivityAsync(string shareToken);
}