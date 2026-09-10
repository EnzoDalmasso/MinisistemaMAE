using Clinica.Aplicacion.DTOs.Profesionales;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioProfesionales
{
    Task<List<ProfesionalDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<ProfesionalDto> CrearAsync(CrearProfesionalDto dto, CancellationToken cancellationToken = default);
    Task<ProfesionalDto> ActualizarAsync(int id, ActualizarProfesionalDto dto, CancellationToken cancellationToken = default);
}
