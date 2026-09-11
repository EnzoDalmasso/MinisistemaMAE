using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinica.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarBloquesHorarioProfesional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloquesHorarioProfesional",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfesionalId = table.Column<int>(type: "integer", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloquesHorarioProfesional", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloquesHorarioProfesional_Profesionales_ProfesionalId",
                        column: x => x.ProfesionalId,
                        principalTable: "Profesionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloquesHorarioProfesional_ProfesionalId_DiaSemana",
                table: "BloquesHorarioProfesional",
                columns: new[] { "ProfesionalId", "DiaSemana" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloquesHorarioProfesional");
        }
    }
}
