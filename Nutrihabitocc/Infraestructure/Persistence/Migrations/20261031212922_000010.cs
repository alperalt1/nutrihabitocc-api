using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrihabitocc.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _000010 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "PlantillasFormulario"
                SET "Estructura" = "Estructura"->'estructura',
                    "ModifiedBy" = 'DataFix',
                    "ModifiedOn" = NOW()
                WHERE jsonb_typeof("Estructura") = 'object'
                  AND "Estructura" ? 'estructura'
                  AND jsonb_typeof("Estructura"->'estructura') = 'object'
                  AND NOT ("Estructura" ? 'secciones')
                  AND NOT ("Estructura" ? 'sections');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // La corrección de datos no se revierte automáticamente.
        }
    }
}
