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

    // Autogestión del paciente: solo puede tocar fecha/horario de su propio turno.
    Task<TurnoDto> ReprogramarAsync(int id, ReprogramarTurnoDto dto, CancellationToken cancellationToken = default);

    // Usado por el profesional para marcar Confirmado/Atendido/Cancelado en
    // sus propios turnos (y por el administrador sobre cualquiera).
    Task<TurnoDto> CambiarEstadoAsync(int id, CambiarEstadoTurnoDto dto, CancellationToken cancellationToken = default);

    // Grilla de horarios libres de un profesional en una fecha, según su
    // duración de turno configurada. La usan las 3 pantallas que permiten
    // elegir horario (pedir turno, nuevo turno del admin, reprogramar).
    // "idExcluirTurno" evita que el propio turno que se está editando figure
    // como "ocupado" por sí mismo.
    Task<List<TimeOnly>> ObtenerHorariosDisponiblesAsync(int profesionalId, DateOnly fecha, int? idExcluirTurno = null, CancellationToken cancellationToken = default);
}
