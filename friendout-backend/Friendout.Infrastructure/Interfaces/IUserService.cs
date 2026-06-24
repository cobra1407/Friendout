using System;
using System.Threading.Tasks;
using Friendout.Domain.DTOs.User;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Returns the user with the given provider and provider account id.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="providerAccountId"></param>
    /// <returns></returns>
    Task<ServiceResult<User>> GetUserByProviderAccountIdAsync(ProviderEnum provider, string providerAccountId);

    /// <summary>
    /// Creates a new user from OAuth data.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="providerId"></param>
    /// <param name="username"></param>
    /// <param name="email"></param>
    /// <param name="avatarUrl"></param>
    /// <returns></returns>
    Task<ServiceResult<User>> CreateUserFromOAuthAsync(
        ProviderEnum provider,
        string providerId, 
        string username, 
        string? email, 
        string? avatarUrl);
    
    /// <summary>
    /// Returns the email of the user with the given id.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<ServiceResult<string>> GetUserEmailAsync(string userId);

    /// <summary>
    /// Returns the authenticated user's profile (name, email, avatar).
    /// </summary>
    Task<ServiceResult<UserProfileDto>> GetUserProfileAsync(string userId);

    /// <summary>
    /// Updates the authenticated user's editable profile fields (name only for now).
    /// </summary>
    Task<ServiceResult<UserProfileDto>> UpdateUserProfileAsync(string userId, UpdateUserProfileDto dto);

    /// <summary>
    /// Uploads and sets a custom avatar for the authenticated user, replacing any previous
    /// custom avatar on disk. The original OAuth avatar is preserved separately and can be
    /// restored via ResetAvatarAsync.
    /// </summary>
    Task<ServiceResult<UserProfileDto>> UploadAvatarAsync(string userId, FileUpload avatar);

    /// <summary>
    /// Resets the authenticated user's avatar back to the original OAuth-provided avatar,
    /// deleting any custom uploaded avatar from disk.
    /// </summary>
    Task<ServiceResult<UserProfileDto>> ResetAvatarAsync(string userId);
} 