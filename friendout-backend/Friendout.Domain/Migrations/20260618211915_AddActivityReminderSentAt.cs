using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "reminder_sent_at",
                table: "activities",
                type: "datetime(3)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_activities_start_at_reminder_sent_at",
                table: "activities",
                columns: new[] { "start_at", "reminder_sent_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_activities_start_at_reminder_sent_at",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "reminder_sent_at",
                table: "activities");
        }
    }
}
