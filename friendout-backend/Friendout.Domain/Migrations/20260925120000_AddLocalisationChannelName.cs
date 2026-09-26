using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendout.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalisationChannelName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "channel_name",
                table: "localisations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel_name",
                table: "localisations");
        }
    }
}
