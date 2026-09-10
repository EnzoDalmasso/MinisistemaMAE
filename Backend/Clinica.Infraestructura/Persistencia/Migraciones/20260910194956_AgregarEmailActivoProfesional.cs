using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarEmailActivoProfesional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Profesionales",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Profesionales",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDesactivacion",
                table: "Profesionales",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Profesionales");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Profesionales");

            migrationBuilder.DropColumn(
                name: "FechaDesactivacion",
                table: "Profesionales");
        }
    }
}
