using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioProfesionales
{
    Task<List<Profesional>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Profesional?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Profesional profesional, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Profesional profesional, CancellationToken cancellationToken = default);
}
