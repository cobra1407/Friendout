using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityPublicSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_publicly_shared",
                table: "activities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "share_token",
                table: "activities",
                type: "varchar(64)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_activities_share_token",
                table: "activities",
                column: "share_token",
                unique: true,
                filter: "`share_token` IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_activities_share_token",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "is_publicly_shared",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "share_token",
                table: "activities");
        }
    }
}
