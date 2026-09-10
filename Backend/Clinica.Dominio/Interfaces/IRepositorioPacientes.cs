using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioPacientes
{
    Task<List<Paciente>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Paciente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    // Cada método de escritura persiste sus cambios (llama a SaveChanges internamente):
    // no se usa un Unit of Work aparte porque el DbContext de EF Core ya cumple ese rol
    // y cada operación de este sistema afecta un único agregado por vez.
    Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Paciente paciente, CancellationToken cancellationToken = default);
}
