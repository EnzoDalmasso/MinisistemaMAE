using Clinica.Aplicacion.DTOs.Profesionales;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioProfesionales
{
    Task<List<ProfesionalDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<ProfesionalDto> CrearAsync(CrearProfesionalDto dto, CancellationToken cancellationToken = default);
    Task<ProfesionalDto> ActualizarAsync(int id, ActualizarProfesionalDto dto, CancellationToken cancellationToken = default);

    // Puede llamarlo el Administrador (sobre cualquier profesional) o el
    // propio profesional (solo sobre sí mismo); el chequeo de permiso vive acá.
    Task<ProfesionalDto> ActualizarDuracionTurnoAsync(int id, ActualizarDuracionTurnoDto dto, CancellationToken cancellationToken = default);
}
