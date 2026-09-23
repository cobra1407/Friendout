using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountDeletionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_deletion_requests",
                columns: table => new
                {
                    token = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_deletion_requests", x => x.token);
                    table.ForeignKey(
                        name: "FK_account_deletion_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_account_deletion_requests_expires_at",
                table: "account_deletion_requests",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_account_deletion_requests_user_id",
                table: "account_deletion_requests",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_deletion_requests");
        }
    }
}
