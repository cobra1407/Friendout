using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityIdToUserEquipmentPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_equipment",
                table: "user_equipment");

            migrationBuilder.DropIndex(
                name: "IX_user_equipment_user_id",
                table: "user_equipment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_equipment",
                table: "user_equipment",
                columns: new[] { "user_id", "equipment_id", "activity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_participations_user_id_activity_id_sub_activity_id",
                table: "user_participations",
                columns: new[] { "user_id", "activity_id", "sub_activity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_equipment_activity_id",
                table: "user_equipment",
                column: "activity_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_equipment_activities_activity_id",
                table: "user_equipment",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_equipment_activities_activity_id",
                table: "user_equipment");

            migrationBuilder.DropIndex(
                name: "IX_user_participations_user_id_activity_id_sub_activity_id",
                table: "user_participations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_equipment",
                table: "user_equipment");

            migrationBuilder.DropIndex(
                name: "IX_user_equipment_activity_id",
                table: "user_equipment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_equipment",
                table: "user_equipment",
                columns: new[] { "user_id", "equipment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_equipment_user_id",
                table: "user_equipment",
                column: "user_id");
        }
    }
}
