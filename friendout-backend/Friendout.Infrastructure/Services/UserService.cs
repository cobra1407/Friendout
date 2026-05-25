using System;
using System.Linq;
using System.Threading.Tasks;
using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;
using Friendout.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Friendout.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly FriendoutDbContext _friendoutDbContext;

    public UserService(FriendoutDbContext friendoutDbContext)
    {
        _friendoutDbContext = friendoutDbContext;
    }

    /// <summary>
    /// Returns true if no user exists in the database yet (used to assign Admin to the first user).
    /// </summary>
    private async Task<bool> IsFirstUserAsync()
    {
        return !await _friendoutDbContext.Users.AnyAsync();
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
            return ServiceResult<User>.Failure("User not found for this provider.");

        return ServiceResult<User>.Success(account.User);
    }

    /// <summary>
    /// Creates or retrieves a user from an OAuth login.
    /// 
    /// Three scenarios are handled:
    ///   1. The OAuth account (provider + providerId) already exists → return the linked user.
    ///   2. The OAuth account is new but the email is already used by another account
    ///      → link this new provider to the existing user (account linking).
    ///   3. Completely new user → create user + account.
    /// </summary>
    public async Task<ServiceResult<User>> CreateUserFromOAuthAsync(
        ProviderEnum provider,
        string providerId,
        string username,
        string? email,
        string? avatarUrl)
    {
        var strategy = _friendoutDbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _friendoutDbContext.Database.BeginTransactionAsync();

            var providerValue = provider.GetEnumMemberValue();

            // 1. The OAuth account already exists → return its user directly.
            var existingAccount = await _friendoutDbContext.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a =>
                    a.Provider == providerValue &&
                    a.ProviderAccountId == providerId);

            if (existingAccount != null)
            {
                await transaction.CommitAsync();
                return ServiceResult<User>.Success(existingAccount.User);
            }

            // 2. Different provider but same email → link to existing user.
            User? user = null;

            if (!string.IsNullOrEmpty(email))
            {
                user = await _friendoutDbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }

            if (user != null)
            {
                // Link the new OAuth provider to the existing account.
                var linkedAccount = new Account
                {
                    Provider = providerValue,
                    ProviderAccountId = providerId,
                    UserId = user.Id
                };

                _friendoutDbContext.Accounts.Add(linkedAccount);
                await _friendoutDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult<User>.Success(user);
            }

            // 3. Brand-new user: create user then account.
            var isFirstUser = await IsFirstUserAsync();

            user = new User
            {
                Name = username,
                Email = email,
                Role = isFirstUser ? UserRole.Admin : UserRole.User,
                AvatarUrl = avatarUrl
            };

            _friendoutDbContext.Users.Add(user);
            await _friendoutDbContext.SaveChangesAsync();

            var account = new Account
            {
                Provider = providerValue,
                ProviderAccountId = providerId,
                UserId = user.Id
            };

            _friendoutDbContext.Accounts.Add(account);
            await _friendoutDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return ServiceResult<User>.Success(user);
        });
    }
    
    public async Task<ServiceResult<string>> GetUserEmailAsync(string  userId)
    {
        var user = await _friendoutDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        // check if user exists
        if (user is null)
        {
            return ServiceResult<string>.Failure("User not found");
        }

        if (user.Email != null) return ServiceResult<string>.Success(user.Email);
        
        return ServiceResult<string>.Failure("User has no email");
    }
}
