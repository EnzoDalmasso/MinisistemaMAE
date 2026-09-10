using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Persistencia.Configuraciones;

public class PacienteConfiguracion : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellido).IsRequired().HasMaxLength(100);

        // Nullable: el alta autogestionada del paciente (Nombre + Apellido +
        // DNI) no pide estos datos; el alta manual del Administrador sí los
        // sigue exigiendo a través de su propio validador.
        builder.Property(p => p.Telefono).HasMaxLength(30);
        builder.Property(p => p.ObraSocial).HasMaxLength(100);
        builder.Property(p => p.Email).HasMaxLength(150);

        builder.Property(p => p.Dni).HasMaxLength(15);
        // Único cuando está presente: Postgres permite múltiples NULL en un
        // índice único (los pacientes cargados por el admin sin DNI no chocan entre sí).
        builder.HasIndex(p => p.Dni).IsUnique();

        builder.Property(p => p.FechaCreacion).IsRequired();
    }
}
