using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrihabitocc.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _000008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_UserId_WorkspaceId",
                table: "WorkspaceMembers");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_WorkspaceId",
                table: "WorkspaceMembers");

            migrationBuilder.AlterColumn<string>(
                name: "MemberRole",
                table: "WorkspaceMembers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_UserId_WorkspaceId",
                table: "WorkspaceMembers",
                columns: new[] { "UserId", "WorkspaceId" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_WorkspaceId",
                table: "WorkspaceMembers",
                columns: new[] { "WorkspaceId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_WorkspaceId_Paciente",
                table: "WorkspaceMembers",
                column: "WorkspaceId",
                unique: true,
                filter: "\"IsActive\" = true AND \"MemberRole\" = 'Paciente'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_UserId_WorkspaceId",
                table: "WorkspaceMembers");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_WorkspaceId",
                table: "WorkspaceMembers");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceMembers_WorkspaceId_Paciente",
                table: "WorkspaceMembers");

            migrationBuilder.AlterColumn<string>(
                name: "MemberRole",
                table: "WorkspaceMembers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_UserId_WorkspaceId",
                table: "WorkspaceMembers",
                columns: new[] { "UserId", "WorkspaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceMembers_WorkspaceId",
                table: "WorkspaceMembers",
                column: "WorkspaceId");
        }
    }
}
