using Clinica.Dominio.Entidades;
using Clinica.Dominio.Interfaces;
using Clinica.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Persistencia.Repositorios;

public class RepositorioProfesionales : IRepositorioProfesionales
{
    private readonly ClinicaDbContext _contexto;

    public RepositorioProfesionales(ClinicaDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<Profesional>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await _contexto.Profesionales
            .AsNoTracking()
            .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Profesional?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _contexto.Profesionales.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AgregarAsync(Profesional profesional, CancellationToken cancellationToken = default)
    {
        _contexto.Profesionales.Add(profesional);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Profesional profesional, CancellationToken cancellationToken = default)
    {
        _contexto.Profesionales.Update(profesional);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
