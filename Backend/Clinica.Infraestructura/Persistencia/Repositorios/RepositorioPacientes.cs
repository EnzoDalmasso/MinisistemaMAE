using Clinica.Dominio.Entidades;
using Clinica.Dominio.Interfaces;
using Clinica.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Persistencia.Repositorios;

public class RepositorioPacientes : IRepositorioPacientes
{
    private readonly ClinicaDbContext _contexto;

    public RepositorioPacientes(ClinicaDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<Paciente>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await _contexto.Pacientes
            .AsNoTracking()
            .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

    public async Task<Paciente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _contexto.Pacientes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default) =>
        await _contexto.Pacientes.FirstOrDefaultAsync(p => p.Dni == dni, cancellationToken);

    public async Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        _contexto.Pacientes.Add(paciente);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        _contexto.Pacientes.Update(paciente);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
