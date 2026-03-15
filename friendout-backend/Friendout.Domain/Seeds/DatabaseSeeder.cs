using Friendout.Domain.Context;

namespace Friendout.Domain.Seeds
{
    /// <summary>
    /// Orchestrates the seeding of the database for one or multiple tables.
    /// </summary>
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Seeds multiple tables specified in the <paramref name="tables"/> collection.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="tables">A collection of table names to seed. If empty, nothing is seeded.</param>
        public static async Task SeedTablesAsync(FriendoutDbContext db, ICollection<string> tables)
        {
            if (tables.Count == 0)
                return;

            foreach (var table in tables)
            {
                await SeedAsync(db, table);
            }
        }
        
        /// <summary>
        /// Seeds a specific table or all tables if <paramref name="table"/> is null.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="table">
        /// The name of the table to seed (e.g., "users", "activities").
        /// If null, all available tables are seeded.
        /// </param>
        public static async Task SeedAsync(FriendoutDbContext db, string? table = null)
        {
            switch (table?.ToLower())
            {
                case "users":
                    await UserSeeder.SeedAsync(db);
                    break;
                case "activities":
                    await ActivitySeeder.SeedAsync(db);
                    break;
                case "wishlists":
                    // await WishlistSeeder.Seed(db);
                    break;
                default:
                    await UserSeeder.SeedAsync(db);
                    await ActivitySeeder.SeedAsync(db);
                    //await WishlistSeeder.Seed(db);
                    // ajouter d'autres seeders ici
                    break;
            }
        }
    }
}