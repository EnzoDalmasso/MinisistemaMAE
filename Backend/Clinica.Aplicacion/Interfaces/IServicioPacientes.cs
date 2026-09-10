using Clinica.Aplicacion.DTOs.Pacientes;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioPacientes
{
    Task<List<PacienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default);
    Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken cancellationToken = default);

    // Autoservicio: el propio paciente consulta/completa sus datos de
    // contacto (nunca un id ajeno, ver ServicioPacientes).
    Task<PacienteDto> ObtenerPropioAsync(CancellationToken cancellationToken = default);
    Task<PacienteDto> ActualizarContactoPropioAsync(ActualizarContactoPacienteDto dto, CancellationToken cancellationToken = default);
}
