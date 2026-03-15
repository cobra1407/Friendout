using Friendout.Domain.Context;
using Friendout.Domain.Models;
using Bogus;
using Friendout.Domain.Enums;

namespace Friendout.Domain.Seeds;

public static class ActivitySeeder
{
    public static async Task SeedAsync(FriendoutDbContext db)
    {
        int amountActivityToSeed = 20;

        if (!db.Activities.Any())
        {
            var user = db.Users.FirstOrDefault();
            string userId = user?.Id ?? Guid.NewGuid().ToString(); // fallback si aucun user

            var faker = new Faker<Activity>()
                .RuleFor(a => a.Id, f => Guid.NewGuid().ToString())
                .RuleFor(a => a.Title, f => f.Lorem.Sentence(3))
                .RuleFor(a => a.Description, f => f.Lorem.Paragraph(1))
                .RuleFor(a => a.StartAt, f => f.Date.Future())
                .RuleFor(a => a.EndAt, (f, a) => a.StartAt.AddHours(2))
                .RuleFor(a => a.EstimatedPrice, f => f.Random.Double(0, 200))
                .RuleFor(a => a.ImageId, f => null)
                .RuleFor(a => a.CreatedBy, f => userId)
                .RuleFor(a => a.CreatedAt, f => DateTime.UtcNow)
                .RuleFor(a => a.UpdatedAt, f => DateTime.UtcNow);

            for (int i = 0; i < amountActivityToSeed; i++)
            {
                // Créer une localisation fictive pour chaque activité
                var localisation = new Localisation
                {
                    Type = LocalisationType.Address,
                    Address = new Faker().Address.FullAddress()
                };
                db.Localisations.Add(localisation);

                var activity = faker.Generate();
                activity.Localisation = localisation; // Associer la localisation
                db.Activities.Add(activity);
            }

            await db.SaveChangesAsync();
        }
    }

}