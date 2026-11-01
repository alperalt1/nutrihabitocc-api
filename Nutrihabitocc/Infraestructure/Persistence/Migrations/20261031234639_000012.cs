using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Nutrihabitocc.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _000012 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "menu_mensual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteUserId = table.Column<int>(type: "integer", nullable: false),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlantillaOrigenId = table.Column<int>(type: "integer", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreadoPorUserId = table.Column<int>(type: "integer", nullable: true),
                    PublicadoPorUserId = table.Column<int>(type: "integer", nullable: true),
                    PublicadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_mensual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_mensual_Users_PacienteUserId",
                        column: x => x.PacienteUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_menu_mensual_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_plantilla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: true),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlantillaOrigenId = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_plantilla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_plantilla_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "receta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: true),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Instrucciones = table.Column<string>(type: "text", nullable: true),
                    PorcionesBase = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RecetaOrigenId = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_receta_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "menu_dia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuMensualId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    MaxComidas = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_dia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_dia_menu_mensual_MenuMensualId",
                        column: x => x.MenuMensualId,
                        principalTable: "menu_mensual",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_plantilla_dia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuPlantillaId = table.Column<int>(type: "integer", nullable: false),
                    Dia = table.Column<int>(type: "integer", nullable: false),
                    MaxComidas = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_plantilla_dia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_plantilla_dia_menu_plantilla_MenuPlantillaId",
                        column: x => x.MenuPlantillaId,
                        principalTable: "menu_plantilla",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "receta_ingrediente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecetaId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<long>(type: "bigint", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Unidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receta_ingrediente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_receta_ingrediente_producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receta_ingrediente_receta_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "receta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_comida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuDiaId = table.Column<int>(type: "integer", nullable: false),
                    TipoComida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    RecetaId = table.Column<int>(type: "integer", nullable: true),
                    RecetaSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    Porciones = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_comida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_comida_menu_dia_MenuDiaId",
                        column: x => x.MenuDiaId,
                        principalTable: "menu_dia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_comida_receta_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "receta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "menu_plantilla_comida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuPlantillaDiaId = table.Column<int>(type: "integer", nullable: false),
                    TipoComida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    RecetaId = table.Column<int>(type: "integer", nullable: true),
                    Porciones = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_plantilla_comida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_plantilla_comida_menu_plantilla_dia_MenuPlantillaDiaId",
                        column: x => x.MenuPlantillaDiaId,
                        principalTable: "menu_plantilla_dia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menu_plantilla_comida_receta_RecetaId",
                        column: x => x.RecetaId,
                        principalTable: "receta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_comida_RecetaId",
                table: "menu_comida",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuComida_Dia_Orden",
                table: "menu_comida",
                columns: new[] { "MenuDiaId", "Orden" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_MenuDia_Menu_Fecha",
                table: "menu_dia",
                columns: new[] { "MenuMensualId", "Fecha" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_menu_mensual_WorkspaceId",
                table: "menu_mensual",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuMensual_Paciente_Anio_Mes",
                table: "menu_mensual",
                columns: new[] { "PacienteUserId", "Anio", "Mes" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_menu_plantilla_WorkspaceId",
                table: "menu_plantilla",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_plantilla_comida_RecetaId",
                table: "menu_plantilla_comida",
                column: "RecetaId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuPlantillaComida_Dia_Orden",
                table: "menu_plantilla_comida",
                columns: new[] { "MenuPlantillaDiaId", "Orden" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_MenuPlantillaDia_Plantilla_Dia",
                table: "menu_plantilla_dia",
                columns: new[] { "MenuPlantillaId", "Dia" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_receta_WorkspaceId",
                table: "receta",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_receta_ingrediente_ProductoId",
                table: "receta_ingrediente",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetaIngrediente_Receta_Producto",
                table: "receta_ingrediente",
                columns: new[] { "RecetaId", "ProductoId" },
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "menu_comida");

            migrationBuilder.DropTable(
                name: "menu_plantilla_comida");

            migrationBuilder.DropTable(
                name: "receta_ingrediente");

            migrationBuilder.DropTable(
                name: "menu_dia");

            migrationBuilder.DropTable(
                name: "menu_plantilla_dia");

            migrationBuilder.DropTable(
                name: "receta");

            migrationBuilder.DropTable(
                name: "menu_mensual");

            migrationBuilder.DropTable(
                name: "menu_plantilla");
        }
    }
}
