using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Persistencia.Configuraciones;

public class TurnoConfiguracion : IEntityTypeConfiguration<Turno>
{
    // Nombre del índice usado también por RepositorioTurnos para reconocer,
    // ante un DbUpdateException, si la violación corresponde puntualmente a
    // esta restricción de disponibilidad (y no a otra restricción única).
    public const string NombreIndiceDisponibilidad = "IX_Turnos_Disponibilidad";

    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Fecha).IsRequired();
        builder.Property(t => t.Horario).IsRequired();
        builder.Property(t => t.Estado).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.FechaCreacion).IsRequired();

        builder.HasOne(t => t.Paciente)
            .WithMany(p => p.Turnos)
            .HasForeignKey(t => t.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Profesional)
            .WithMany(p => p.Turnos)
            .HasForeignKey(t => t.ProfesionalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Regla de negocio crítica: un profesional no puede tener dos turnos
        // activos en la misma fecha y horario. Se implementa como índice único
        // FILTRADO (parcial): solo abarca turnos con Estado <> 'Cancelado', que es
        // la decisión de negocio documentada (un turno cancelado libera el horario).
        // Esta restricción es la red de seguridad definitiva ante condiciones de
        // carrera; la verificación en ServicioTurnos es solo la comprobación optimista
        // que da un mensaje de error claro en el caso normal (sin concurrencia).
        builder.HasIndex(t => new { t.ProfesionalId, t.Fecha, t.Horario })
            .IsUnique()
            .HasFilter("\"Estado\" <> 'Cancelado'")
            .HasDatabaseName(NombreIndiceDisponibilidad);

        // Índice de apoyo para los filtros de listado por fecha (no es una FK,
        // así que EF Core no lo crea automáticamente como sí hace con las FK).
        builder.HasIndex(t => t.Fecha).HasDatabaseName("IX_Turnos_Fecha");

        // El token de concurrencia optimista basado en "xmin" se configura en
        // ClinicaDbContext (solo cuando el proveedor es Npgsql): es un concepto
        // específico de PostgreSQL y no tiene sentido para el proveedor SQLite
        // que usan los tests en memoria.
    }
}
