using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrihabitocc.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _000013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MenuMensual_Paciente_Anio_Mes",
                table: "menu_mensual");

            migrationBuilder.CreateIndex(
                name: "IX_MenuMensual_Paciente_Workspace_Anio_Mes",
                table: "menu_mensual",
                columns: new[] { "PacienteUserId", "WorkspaceId", "Anio", "Mes" },
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MenuMensual_Paciente_Workspace_Anio_Mes",
                table: "menu_mensual");

            migrationBuilder.CreateIndex(
                name: "IX_MenuMensual_Paciente_Anio_Mes",
                table: "menu_mensual",
                columns: new[] { "PacienteUserId", "Anio", "Mes" },
                unique: true,
                filter: "\"IsActive\" = true");
        }
    }
}
