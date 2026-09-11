using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Persistencia.Configuraciones;

public class BloqueHorarioProfesionalConfiguracion : IEntityTypeConfiguration<BloqueHorarioProfesional>
{
    public void Configure(EntityTypeBuilder<BloqueHorarioProfesional> builder)
    {
        builder.ToTable("BloquesHorarioProfesional");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.DiaSemana).IsRequired();
        builder.Property(b => b.HoraInicio).IsRequired();
        builder.Property(b => b.HoraFin).IsRequired();

        builder.HasOne(b => b.Profesional)
            .WithMany(p => p.BloquesHorario)
            .HasForeignKey(b => b.ProfesionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.ProfesionalId, b.DiaSemana });
    }
}
