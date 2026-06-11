using Microsoft.EntityFrameworkCore;
using Friendout.Domain.Models;

namespace Friendout.Domain.Context
{
    public class FriendoutDbContext(DbContextOptions<FriendoutDbContext> options) : DbContext(options)
    {
        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<SubActivity> SubActivities { get; set; }
        public DbSet<UserParticipation> UserParticipation { get; set; }
        public DbSet<ActivityComment> Comments { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<UserEquipment> UserEquipment { get; set; }
        public DbSet<ActivityEquipment> ActivityEquipment { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<VerificationToken> VerificationTokens { get; set; }
        public DbSet<Localisation> Localisations { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AllowedGuild> AllowedGuilds { get; set; }
        public DbSet<AllowedEmail> AllowedEmails { get; set; }
        public DbSet<AccessRequest> AccessRequests { get; set; }
        public DbSet<AppLog> AppLogs { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<UserNotificationPreferences> UserNotificationPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(e => e.Accounts)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Sessions)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.CreatedActivities)
                    .WithOne(e => e.Creator)
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.UserParticipation)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Comments)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.UserEquipments)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.UserAchievements)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Preferences)
                    .WithOne(e => e.User)
                    .HasForeignKey<UserPreferences>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.NotificationPreferences)
                    .WithOne(e => e.User)
                    .HasForeignKey<UserNotificationPreferences>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => new { e.Provider, e.ProviderAccountId }).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => e.StartAt);

                entity.HasOne(e => e.Image)
                    .WithMany()
                    .HasForeignKey(e => e.ImageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.SubActivities)
                    .WithOne(e => e.Activity)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.UserParticipations)
                    .WithOne(e => e.Activity)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Comments)
                    .WithOne(e => e.Activity)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ActivityEquipments)
                    .WithOne(e => e.Activity)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SubActivity>(entity =>
            {
                entity.HasIndex(e => e.ActivityId);

                entity.HasMany(e => e.UserParticipations)
                    .WithOne(e => e.SubActivity)
                    .HasForeignKey(e => e.SubActivityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserParticipation>(entity =>
            {
                entity.HasIndex(e => e.ActivityId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.Id }).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.ActivityId, e.SubActivityId }).IsUnique();

                entity.HasOne(e => e.Activity)
                    .WithMany(e => e.UserParticipations)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SubActivity)
                    .WithMany(e => e.UserParticipations)
                    .HasForeignKey(e => e.SubActivityId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserParticipation)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ActivityComment>(entity =>
            {
                entity.HasIndex(e => e.ActivityId);
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();

                entity.HasMany(e => e.UserEquipments)
                    .WithOne(e => e.Equipment)
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ActivityEquipments)
                    .WithOne(e => e.Equipment)
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserEquipment>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.EquipmentId, e.ActivityId });

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserEquipments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Equipment)
                    .WithMany(e => e.UserEquipments)
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Activity)
                    .WithMany(e => e.UserEquipments)
                    .HasForeignKey(e => e.ActivityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ActivityEquipment>(entity =>
            {
                entity.HasIndex(e => new { e.ActivityId, e.EquipmentId }).IsUnique();
            });

            modelBuilder.Entity<UserAchievement>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.AchievementId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AchievementId);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserAchievements)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Achievement)
                    .WithMany(e => e.UserAchievements)
                    .HasForeignKey(e => e.AchievementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VerificationToken>(entity =>
            {
                entity.HasKey(e => e.Token);
                entity.HasIndex(e => e.Identifier);
                entity.HasIndex(e => e.Expires);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Token);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiresAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AllowedGuild>(entity =>
            {
                entity.HasIndex(e => e.GuildId).IsUnique();
            });

            modelBuilder.Entity<AllowedEmail>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<AppLog>(entity =>
            {
                entity.HasIndex(e => e.Level);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.Level).HasConversion<string>();
            });

            modelBuilder.Entity<AccessRequest>(entity =>
            {
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
            });

            // Seed default access restriction settings.
            modelBuilder.Entity<AppSetting>().HasData(
                new AppSetting { Key = "discord_restricted", Value = "false", UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new AppSetting { Key = "google_restricted",  Value = "false", UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
