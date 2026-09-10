using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Persistencia.Configuraciones;

public class ProfesionalConfiguracion : IEntityTypeConfiguration<Profesional>
{
    public void Configure(EntityTypeBuilder<Profesional> builder)
    {
        builder.ToTable("Profesionales");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Especialidad).IsRequired().HasMaxLength(100);
        builder.Property(p => p.DuracionTurnoMinutos).IsRequired().HasDefaultValue(30);
        builder.Property(p => p.Email).HasMaxLength(150);
        builder.Property(p => p.Activo).IsRequired().HasDefaultValue(true);
        builder.Property(p => p.FechaCreacion).IsRequired();
    }
}
