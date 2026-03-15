using System.Threading.Tasks;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Services;

namespace Friendout.Infrastructure.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Recherche un utilisateur par son compte externe (Discord, Google, etc.)
    /// </summary>
    /// <returns>Success avec User si trouvé, Failure si erreur base</returns>
    Task<ServiceResult<User>> GetUserByProviderAccountIdAsync(ProviderEnum provider, string providerAccountId);

    /// <summary>
    /// Crée un nouvel utilisateur à partir des infos Discord
    /// Applique le rôle ADMIN si c'est le premier utilisateur
    /// </summary>
    Task<ServiceResult<User>> CreateUserFromOAuthAsync(
        ProviderEnum provider,
        string providerId, 
        string username, 
        string? email, 
        string? avatarUrl);
} 