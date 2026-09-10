using Clinica.Aplicacion.DTOs.Turnos;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioTurnos
{
    // El filtrado por rol (profesional ve solo lo suyo) se resuelve adentro del
    // servicio a partir de IUsuarioActual, no confiando en lo que venga en el filtro.
    Task<List<TurnoDto>> ObtenerAsync(TurnoFiltroDto filtro, CancellationToken cancellationToken = default);
    Task<TurnoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TurnoDto> CrearAsync(CrearTurnoDto dto, CancellationToken cancellationToken = default);
    Task<TurnoDto> ActualizarAsync(int id, ActualizarTurnoDto dto, CancellationToken cancellationToken = default);
    Task<TurnoDto> CancelarAsync(int id, CancellationToken cancellationToken = default);
}
