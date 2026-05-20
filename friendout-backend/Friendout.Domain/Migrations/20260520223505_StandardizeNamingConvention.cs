using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_users_userId",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_images_imageId",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_localisation_localisationId",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_users_createdBy",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_comment_activities_activityId",
                table: "Activity_comment");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_comment_users_userId",
                table: "Activity_comment");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipment_activities_activityId",
                table: "activity_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipment_equipment_equipmentId",
                table: "activity_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_users_userId",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sub_activities_activities_activityId",
                table: "sub_activities");

            migrationBuilder.DropForeignKey(
                name: "FK_sub_activities_localisation_localisationId",
                table: "sub_activities");

            migrationBuilder.DropForeignKey(
                name: "FK_user_achievements_achievement_achievementId",
                table: "user_achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_user_achievements_users_userId",
                table: "user_achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_user_equipment_equipment_equipmentId",
                table: "user_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_user_equipment_users_userId",
                table: "user_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_user_participation_activities_activityId",
                table: "user_participation");

            migrationBuilder.DropForeignKey(
                name: "FK_user_participation_sub_activities_subActivityId",
                table: "user_participation");

            migrationBuilder.DropForeignKey(
                name: "FK_user_participation_users_userId",
                table: "user_participation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_participation",
                table: "user_participation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_localisation",
                table: "localisation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Activity_comment",
                table: "Activity_comment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_achievement",
                table: "achievement");

            migrationBuilder.DropColumn(
                name: "name",
                table: "access_requests");

            migrationBuilder.RenameTable(
                name: "user_participation",
                newName: "user_participations");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "localisation",
                newName: "localisations");

            migrationBuilder.RenameTable(
                name: "Activity_comment",
                newName: "activity_comments");

            migrationBuilder.RenameTable(
                name: "achievement",
                newName: "achievements");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "emailVerified",
                table: "users",
                newName: "email_verified");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "avatarUrl",
                table: "users",
                newName: "avatar_url");

            migrationBuilder.RenameColumn(
                name: "equipmentId",
                table: "user_equipment",
                newName: "equipment_id");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "user_equipment",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_equipment_userId",
                table: "user_equipment",
                newName: "IX_user_equipment_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_equipment_equipmentId",
                table: "user_equipment",
                newName: "IX_user_equipment_equipment_id");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "user_achievements",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "unlockedAt",
                table: "user_achievements",
                newName: "unlocked_at");

            migrationBuilder.RenameColumn(
                name: "achievementId",
                table: "user_achievements",
                newName: "achievement_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_achievements_userId_achievementId",
                table: "user_achievements",
                newName: "IX_user_achievements_user_id_achievement_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_achievements_userId",
                table: "user_achievements",
                newName: "IX_user_achievements_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_achievements_achievementId",
                table: "user_achievements",
                newName: "IX_user_achievements_achievement_id");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "sub_activities",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "startTime",
                table: "sub_activities",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "localisationId",
                table: "sub_activities",
                newName: "localisation_id");

            migrationBuilder.RenameColumn(
                name: "endTime",
                table: "sub_activities",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "sub_activities",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "activityId",
                table: "sub_activities",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "IX_sub_activities_localisationId",
                table: "sub_activities",
                newName: "IX_sub_activities_localisation_id");

            migrationBuilder.RenameIndex(
                name: "IX_sub_activities_activityId",
                table: "sub_activities",
                newName: "IX_sub_activities_activity_id");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "sessions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "sessionToken",
                table: "sessions",
                newName: "session_token");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_userId",
                table: "sessions",
                newName: "IX_sessions_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_sessionToken",
                table: "sessions",
                newName: "IX_sessions_session_token");

            migrationBuilder.RenameColumn(
                name: "mimeType",
                table: "images",
                newName: "mime_type");

            migrationBuilder.RenameColumn(
                name: "createdBy",
                table: "images",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "altText",
                table: "images",
                newName: "alt_text");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "activity_equipment",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "equipmentId",
                table: "activity_equipment",
                newName: "equipment_id");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "activity_equipment",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "activityId",
                table: "activity_equipment",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipment_equipmentId",
                table: "activity_equipment",
                newName: "IX_activity_equipment_equipment_id");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipment_activityId_equipmentId",
                table: "activity_equipment",
                newName: "IX_activity_equipment_activity_id_equipment_id");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "activities",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "startAt",
                table: "activities",
                newName: "start_at");

            migrationBuilder.RenameColumn(
                name: "localisationId",
                table: "activities",
                newName: "localisation_id");

            migrationBuilder.RenameColumn(
                name: "imageId",
                table: "activities",
                newName: "image_id");

            migrationBuilder.RenameColumn(
                name: "estimatedPrice",
                table: "activities",
                newName: "estimated_price");

            migrationBuilder.RenameColumn(
                name: "endAt",
                table: "activities",
                newName: "end_at");

            migrationBuilder.RenameColumn(
                name: "createdBy",
                table: "activities",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "activities",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_activities_startAt",
                table: "activities",
                newName: "IX_activities_start_at");

            migrationBuilder.RenameIndex(
                name: "IX_activities_localisationId",
                table: "activities",
                newName: "IX_activities_localisation_id");

            migrationBuilder.RenameIndex(
                name: "IX_activities_imageId",
                table: "activities",
                newName: "IX_activities_image_id");

            migrationBuilder.RenameIndex(
                name: "IX_activities_createdBy",
                table: "activities",
                newName: "IX_activities_created_by");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "accounts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "providerAccountId",
                table: "accounts",
                newName: "provider_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_userId",
                table: "accounts",
                newName: "IX_accounts_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_provider_providerAccountId",
                table: "accounts",
                newName: "IX_accounts_provider_provider_account_id");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "user_participations",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "user_participations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "subActivityId",
                table: "user_participations",
                newName: "sub_activity_id");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "user_participations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "activityId",
                table: "user_participations",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_participation_userId",
                table: "user_participations",
                newName: "IX_user_participations_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_participation_subActivityId",
                table: "user_participations",
                newName: "IX_user_participations_sub_activity_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_participation_id",
                table: "user_participations",
                newName: "IX_user_participations_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_participation_activityId",
                table: "user_participations",
                newName: "IX_user_participations_activity_id");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "refresh_tokens",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "refresh_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "IsRevoked",
                table: "refresh_tokens",
                newName: "is_revoked");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "refresh_tokens",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "refresh_tokens",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_expires_at");

            migrationBuilder.RenameColumn(
                name: "virtualUrl",
                table: "localisations",
                newName: "virtual_url");

            migrationBuilder.RenameColumn(
                name: "mapLink",
                table: "localisations",
                newName: "map_link");

            migrationBuilder.RenameColumn(
                name: "displayName",
                table: "localisations",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "activity_comments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "activity_comments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "activity_comments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "activityId",
                table: "activity_comments",
                newName: "activity_id");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_comment_userId",
                table: "activity_comments",
                newName: "IX_activity_comments_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_comment_activityId",
                table: "activity_comments",
                newName: "IX_activity_comments_activity_id");

            migrationBuilder.AlterColumn<string>(
                name: "token",
                table: "refresh_tokens",
                type: "varchar(191)",
                maxLength: 191,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_participations",
                table: "user_participations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_localisations",
                table: "localisations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_activity_comments",
                table: "activity_comments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_achievements",
                table: "achievements",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_users_user_id",
                table: "accounts",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_images_image_id",
                table: "activities",
                column: "image_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_localisations_localisation_id",
                table: "activities",
                column: "localisation_id",
                principalTable: "localisations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_users_created_by",
                table: "activities",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_comments_activities_activity_id",
                table: "activity_comments",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_comments_users_user_id",
                table: "activity_comments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_equipment_activities_activity_id",
                table: "activity_equipment",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_equipment_equipment_equipment_id",
                table: "activity_equipment",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_users_user_id",
                table: "sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sub_activities_activities_activity_id",
                table: "sub_activities",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sub_activities_localisations_localisation_id",
                table: "sub_activities",
                column: "localisation_id",
                principalTable: "localisations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_achievements_achievements_achievement_id",
                table: "user_achievements",
                column: "achievement_id",
                principalTable: "achievements",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_achievements_users_user_id",
                table: "user_achievements",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_equipment_equipment_equipment_id",
                table: "user_equipment",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_equipment_users_user_id",
                table: "user_equipment",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_participations_activities_activity_id",
                table: "user_participations",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_participations_sub_activities_sub_activity_id",
                table: "user_participations",
                column: "sub_activity_id",
                principalTable: "sub_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_participations_users_user_id",
                table: "user_participations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_users_user_id",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_images_image_id",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_localisations_localisation_id",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activities_users_created_by",
                table: "activities");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_comments_activities_activity_id",
                table: "activity_comments");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_comments_users_user_id",
                table: "activity_comments");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipment_activities_activity_id",
                table: "activity_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipment_equipment_equipment_id",
                table: "activity_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_users_user_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_sessions_users_user_id",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_sub_activities_activities_activity_id",
                table: "sub_activities");

            migrationBuilder.DropForeignKey(
                name: "FK_sub_activities_localisations_localisation_id",
                table: "sub_activities");

            migrationBuilder.DropForeignKey(
                name: "FK_user_achievements_achievements_achievement_id",
                table: "user_achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_user_achievements_users_user_id",
                table: "user_achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_user_equipment_equipment_equipment_id",
                table: "user_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_user_equipment_users_user_id",
                table: "user_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_user_participations_activities_activity_id",
                table: "user_participations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_participations_sub_activities_sub_activity_id",
                table: "user_participations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_participations_users_user_id",
                table: "user_participations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_participations",
                table: "user_participations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_localisations",
                table: "localisations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_activity_comments",
                table: "activity_comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_achievements",
                table: "achievements");

            migrationBuilder.RenameTable(
                name: "user_participations",
                newName: "user_participation");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "localisations",
                newName: "localisation");

            migrationBuilder.RenameTable(
                name: "activity_comments",
                newName: "Activity_comment");

            migrationBuilder.RenameTable(
                name: "achievements",
                newName: "achievement");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "users",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "email_verified",
                table: "users",
                newName: "emailVerified");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "users",
                newName: "avatarUrl");

            migrationBuilder.RenameColumn(
                name: "equipment_id",
                table: "user_equipment",
                newName: "equipmentId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_equipment",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_user_equipment_user_id",
                table: "user_equipment",
                newName: "IX_user_equipment_userId");

            migrationBuilder.RenameIndex(
                name: "IX_user_equipment_equipment_id",
                table: "user_equipment",
                newName: "IX_user_equipment_equipmentId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_achievements",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "unlocked_at",
                table: "user_achievements",
                newName: "unlockedAt");

            migrationBuilder.RenameColumn(
                name: "achievement_id",
                table: "user_achievements",
                newName: "achievementId");

            migrationBuilder.RenameIndex(
                name: "IX_user_achievements_user_id_achievement_id",
                table: "user_achievements",
                newName: "IX_user_achievements_userId_achievementId");

            migrationBuilder.RenameIndex(
                name: "IX_user_achievements_user_id",
                table: "user_achievements",
                newName: "IX_user_achievements_userId");

            migrationBuilder.RenameIndex(
                name: "IX_user_achievements_achievement_id",
                table: "user_achievements",
                newName: "IX_user_achievements_achievementId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "sub_activities",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "sub_activities",
                newName: "startTime");

            migrationBuilder.RenameColumn(
                name: "localisation_id",
                table: "sub_activities",
                newName: "localisationId");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "sub_activities",
                newName: "endTime");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "sub_activities",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "sub_activities",
                newName: "activityId");

            migrationBuilder.RenameIndex(
                name: "IX_sub_activities_localisation_id",
                table: "sub_activities",
                newName: "IX_sub_activities_localisationId");

            migrationBuilder.RenameIndex(
                name: "IX_sub_activities_activity_id",
                table: "sub_activities",
                newName: "IX_sub_activities_activityId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "sessions",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "session_token",
                table: "sessions",
                newName: "sessionToken");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_user_id",
                table: "sessions",
                newName: "IX_sessions_userId");

            migrationBuilder.RenameIndex(
                name: "IX_sessions_session_token",
                table: "sessions",
                newName: "IX_sessions_sessionToken");

            migrationBuilder.RenameColumn(
                name: "mime_type",
                table: "images",
                newName: "mimeType");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "images",
                newName: "createdBy");

            migrationBuilder.RenameColumn(
                name: "alt_text",
                table: "images",
                newName: "altText");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "activity_equipment",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "equipment_id",
                table: "activity_equipment",
                newName: "equipmentId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "activity_equipment",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "activity_equipment",
                newName: "activityId");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipment_equipment_id",
                table: "activity_equipment",
                newName: "IX_activity_equipment_equipmentId");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipment_activity_id_equipment_id",
                table: "activity_equipment",
                newName: "IX_activity_equipment_activityId_equipmentId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "activities",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "start_at",
                table: "activities",
                newName: "startAt");

            migrationBuilder.RenameColumn(
                name: "localisation_id",
                table: "activities",
                newName: "localisationId");

            migrationBuilder.RenameColumn(
                name: "image_id",
                table: "activities",
                newName: "imageId");

            migrationBuilder.RenameColumn(
                name: "estimated_price",
                table: "activities",
                newName: "estimatedPrice");

            migrationBuilder.RenameColumn(
                name: "end_at",
                table: "activities",
                newName: "endAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "activities",
                newName: "createdBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "activities",
                newName: "createdAt");

            migrationBuilder.RenameIndex(
                name: "IX_activities_start_at",
                table: "activities",
                newName: "IX_activities_startAt");

            migrationBuilder.RenameIndex(
                name: "IX_activities_localisation_id",
                table: "activities",
                newName: "IX_activities_localisationId");

            migrationBuilder.RenameIndex(
                name: "IX_activities_image_id",
                table: "activities",
                newName: "IX_activities_imageId");

            migrationBuilder.RenameIndex(
                name: "IX_activities_created_by",
                table: "activities",
                newName: "IX_activities_createdBy");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "accounts",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "provider_account_id",
                table: "accounts",
                newName: "providerAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_user_id",
                table: "accounts",
                newName: "IX_accounts_userId");

            migrationBuilder.RenameIndex(
                name: "IX_accounts_provider_provider_account_id",
                table: "accounts",
                newName: "IX_accounts_provider_providerAccountId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_participation",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "user_participation",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "sub_activity_id",
                table: "user_participation",
                newName: "subActivityId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "user_participation",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "user_participation",
                newName: "activityId");

            migrationBuilder.RenameIndex(
                name: "IX_user_participations_user_id",
                table: "user_participation",
                newName: "IX_user_participation_userId");

            migrationBuilder.RenameIndex(
                name: "IX_user_participations_sub_activity_id",
                table: "user_participation",
                newName: "IX_user_participation_subActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_user_participations_id",
                table: "user_participation",
                newName: "IX_user_participation_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_participations_activity_id",
                table: "user_participation",
                newName: "IX_user_participation_activityId");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "RefreshTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "is_revoked",
                table: "RefreshTokens",
                newName: "IsRevoked");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "RefreshTokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "RefreshTokens",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_user_id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_expires_at",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "virtual_url",
                table: "localisation",
                newName: "virtualUrl");

            migrationBuilder.RenameColumn(
                name: "map_link",
                table: "localisation",
                newName: "mapLink");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "localisation",
                newName: "displayName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Activity_comment",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Activity_comment",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Activity_comment",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "activity_id",
                table: "Activity_comment",
                newName: "activityId");

            migrationBuilder.RenameIndex(
                name: "IX_activity_comments_user_id",
                table: "Activity_comment",
                newName: "IX_Activity_comment_userId");

            migrationBuilder.RenameIndex(
                name: "IX_activity_comments_activity_id",
                table: "Activity_comment",
                newName: "IX_Activity_comment_activityId");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "access_requests",
                type: "varchar(191)",
                maxLength: 191,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(191)",
                oldMaxLength: 191)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_participation",
                table: "user_participation",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_localisation",
                table: "localisation",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activity_comment",
                table: "Activity_comment",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_achievement",
                table: "achievement",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_users_userId",
                table: "accounts",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_images_imageId",
                table: "activities",
                column: "imageId",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_activities_localisation_localisationId",
                table: "activities",
                column: "localisationId",
                principalTable: "localisation",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_users_createdBy",
                table: "activities",
                column: "createdBy",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_comment_activities_activityId",
                table: "Activity_comment",
                column: "activityId",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_comment_users_userId",
                table: "Activity_comment",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_equipment_activities_activityId",
                table: "activity_equipment",
                column: "activityId",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_equipment_equipment_equipmentId",
                table: "activity_equipment",
                column: "equipmentId",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sessions_users_userId",
                table: "sessions",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sub_activities_activities_activityId",
                table: "sub_activities",
                column: "activityId",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sub_activities_localisation_localisationId",
                table: "sub_activities",
                column: "localisationId",
                principalTable: "localisation",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_achievements_achievement_achievementId",
                table: "user_achievements",
                column: "achievementId",
                principalTable: "achievement",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_achievements_users_userId",
                table: "user_achievements",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_equipment_equipment_equipmentId",
                table: "user_equipment",
                column: "equipmentId",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_equipment_users_userId",
                table: "user_equipment",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_participation_activities_activityId",
                table: "user_participation",
                column: "activityId",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_participation_sub_activities_subActivityId",
                table: "user_participation",
                column: "subActivityId",
                principalTable: "sub_activities",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_participation_users_userId",
                table: "user_participation",
                column: "userId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
