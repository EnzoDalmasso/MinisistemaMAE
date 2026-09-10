using Clinica.Aplicacion.DTOs.Pacientes;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioPacientes
{
    Task<List<PacienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default);
    Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken cancellationToken = default);
}
