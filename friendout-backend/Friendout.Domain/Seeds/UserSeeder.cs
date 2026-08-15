using Friendout.Domain.Context;
using Friendout.Domain.Enums;
using Friendout.Domain.Models;

namespace Friendout.Domain.Seeds;

public static class UserSeeder
{
    public static async Task SeedAsync(FriendoutDbContext db)
    {
        int amountUserToSeed = 20;
        
        if (!db.Users.Any())
        {
            var faker = new Bogus.Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Name, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedAt, DateTime.Now)
                .RuleFor(u => u.UpdatedAt, DateTime.Now);

            for (int i = 0; i < amountUserToSeed; i++)
            {
                db.Users.Add(faker.Generate());
            }
            await db.SaveChangesAsync();
        }
    }
}