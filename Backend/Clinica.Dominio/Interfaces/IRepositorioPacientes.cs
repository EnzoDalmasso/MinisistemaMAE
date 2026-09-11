using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioPacientes
{
    Task<List<Paciente>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Paciente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    // Usado por el acceso autogestionado de pacientes (login sin contraseña
    // por DNI): permite saber si ya existe una cuenta para ese DNI.
    Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default);

    // Cada método de escritura persiste sus cambios (llama a SaveChanges internamente):
    // no se usa un Unit of Work aparte porque el DbContext de EF Core ya cumple ese rol
    // y cada operación de este sistema afecta un único agregado por vez.
    Task AgregarAsync(Paciente paciente, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Paciente paciente, CancellationToken cancellationToken = default);

    // Borrado definitivo: solo se invoca tras verificar en el servicio que el
    // paciente no tiene ningún turno asociado (la FK Turno -> Paciente es
    // Restrict a propósito, para no perder ese historial).
    Task EliminarAsync(Paciente paciente, CancellationToken cancellationToken = default);
}
