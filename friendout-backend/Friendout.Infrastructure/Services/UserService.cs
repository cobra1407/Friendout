using System;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Enums;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly FriendoutDbContext _friendoutDbContext;

    
    /// <summary>
    /// Determines whether the current user being created is the first user
    /// in the application database.
    /// </summary>
    /// <remarks>
    /// This method checks if at least one user already exists in the database.
    /// It is typically used during the account creation process to apply
    /// special initialization logic for the very first user (e.g. assigning
    /// an ADMIN role).
    /// </remarks>
    /// <returns>
    /// <c>true</c> if no users exist in the database (first user),
    /// otherwise <c>false</c>.
    /// </returns>
    private async Task<bool> IsFirstUserAsync()
    {
        var hasUsers = await _friendoutDbContext.Users.AnyAsync();
        return !hasUsers;
    }
    
    
    public UserService(FriendoutDbContext friendoutDbContext)
    {
        _friendoutDbContext = friendoutDbContext;
    }

    public async Task<ServiceResult<User>> GetUserByProviderAccountIdAsync(
        ProviderEnum provider,
        string providerAccountId)
    {
        var providerValue = provider.GetEnumMemberValue();

        var account = await _friendoutDbContext.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a =>
                a.Provider == providerValue &&
                a.ProviderAccountId == providerAccountId);

        if (account == null)
        {
            return ServiceResult<User>.Failure("User not found for this provider.");
        }

        return ServiceResult<User>.Success(account.User);
    }


    public async Task<ServiceResult<User>> CreateUserFromOAuthAsync(
        ProviderEnum provider,
        string providerId,
        string username,
        string? email,
        string? avatarUrl
    )
    {
        var strategy = _friendoutDbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _friendoutDbContext.Database.BeginTransactionAsync();

            // 1. VÃ©rifier si le compte existe
            var existingAccount = await _friendoutDbContext.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a =>
                    a.Provider == provider.GetEnumMemberValue()
                    && a.ProviderAccountId == providerId
                );

            if (existingAccount != null)
            {
                return ServiceResult<User>.Success(existingAccount.User);
            }

            // 2. CrÃ©er l'utilisateur
            var isFirstUser = !await _friendoutDbContext.Users.AnyAsync();

            var user = new User
            {
                Name = username,
                Email = email,
                Role = isFirstUser ? UserRole.Admin : UserRole.User,
                AvatarUrl = avatarUrl
            };

            _friendoutDbContext.Users.Add(user);
            await _friendoutDbContext.SaveChangesAsync();

            // 3. CrÃ©er le lien OAuth
            var account = new Account
            {
                Provider = provider.GetEnumMemberValue(),
                ProviderAccountId = providerId,
                UserId = user.Id
            };

            _friendoutDbContext.Accounts.Add(account);
            await _friendoutDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return ServiceResult<User>.Success(user);
        });
    }
}
