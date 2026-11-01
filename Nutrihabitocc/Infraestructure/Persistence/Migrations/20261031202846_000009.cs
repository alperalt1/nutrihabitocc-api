using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrihabitocc.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _000009 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "RefreshTokens",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "web");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "RefreshTokens");
        }
    }
}
