using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipment_activities_activity_id",
                table: "activity_equipment");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipment_equipment_equipment_id",
                table: "activity_equipment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_activity_equipment",
                table: "activity_equipment");

            migrationBuilder.RenameTable(
                name: "activity_equipment",
                newName: "activity_equipments");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipment_equipment_id",
                table: "activity_equipments",
                newName: "IX_activity_equipments_equipment_id");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipment_activity_id_equipment_id",
                table: "activity_equipments",
                newName: "IX_activity_equipments_activity_id_equipment_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_activity_equipments",
                table: "activity_equipments",
                column: "id");

            migrationBuilder.CreateTable(
                name: "equipment_lists",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(191)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(191)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(191)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_lists", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_lists_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "equipment_list_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(191)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    equipment_list_id = table.Column<string>(type: "varchar(191)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(191)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_list_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipment_list_items_equipment_lists_equipment_list_id",
                        column: x => x.equipment_list_id,
                        principalTable: "equipment_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_list_items_equipment_list_id",
                table: "equipment_list_items",
                column: "equipment_list_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_lists_user_id",
                table: "equipment_lists",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_lists_user_id_name",
                table: "equipment_lists",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_equipments_activities_activity_id",
                table: "activity_equipments",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_equipments_equipment_equipment_id",
                table: "activity_equipments",
                column: "equipment_id",
                principalTable: "equipment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipments_activities_activity_id",
                table: "activity_equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_equipments_equipment_equipment_id",
                table: "activity_equipments");

            migrationBuilder.DropTable(
                name: "equipment_list_items");

            migrationBuilder.DropTable(
                name: "equipment_lists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_activity_equipments",
                table: "activity_equipments");

            migrationBuilder.RenameTable(
                name: "activity_equipments",
                newName: "activity_equipment");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipments_equipment_id",
                table: "activity_equipment",
                newName: "IX_activity_equipment_equipment_id");

            migrationBuilder.RenameIndex(
                name: "IX_activity_equipments_activity_id_equipment_id",
                table: "activity_equipment",
                newName: "IX_activity_equipment_activity_id_equipment_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_activity_equipment",
                table: "activity_equipment",
                column: "id");

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
        }
    }
}
