using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ConvertProviderToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_users_created_by",
                table: "activities");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "activities",
                type: "varchar(191)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(191)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_users_created_by",
                table: "activities",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activities_users_created_by",
                table: "activities");

            migrationBuilder.UpdateData(
                table: "activities",
                keyColumn: "created_by",
                keyValue: null,
                column: "created_by",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "activities",
                type: "varchar(191)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(191)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_activities_users_created_by",
                table: "activities",
                column: "created_by",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
