using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infraestructura.Persistencia.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
        // Restricción única a nivel de base de datos: dos usuarios no pueden
        // compartir nombre de usuario, sin depender solo de la validación en memoria.
        builder.HasIndex(u => u.NombreUsuario).IsUnique();

        builder.Property(u => u.ContrasenaHash).IsRequired();

        // Se guarda como texto (no como número) para que la tabla sea legible
        // directamente en la base de datos durante la demo/evaluación.
        builder.Property(u => u.Rol).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.HasOne(u => u.Profesional)
            .WithMany()
            .HasForeignKey(u => u.ProfesionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
