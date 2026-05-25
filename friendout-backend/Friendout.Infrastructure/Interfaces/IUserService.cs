using System;
using System.Threading.Tasks;
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
} 