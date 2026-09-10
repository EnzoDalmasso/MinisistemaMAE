using Clinica.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Persistencia.Contexto;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Profesional> Profesionales => Set<Profesional>();
    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicaDbContext).Assembly);

        // Token de concurrencia optimista sobre la columna de sistema "xmin" de
        // PostgreSQL. Se condiciona al proveedor porque "xmin" no existe en SQLite
        // (usado por los tests en memoria) ni tendría sentido allí.
        if (Database.IsNpgsql())
        {
            modelBuilder.Entity<Turno>().Property<uint>("xmin").IsRowVersion();
        }
    }
}
