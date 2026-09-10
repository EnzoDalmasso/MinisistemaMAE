using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioTurnos
{
    // Trae el turno con Paciente y Profesional cargados (para proyectar a DTO sin N+1).
    Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    // Búsqueda con filtros opcionales, usada tanto por el listado general (admin)
    // como por el listado acotado a un profesional/paciente y por el resumen
    // de "próximos turnos". "tomar" limita la cantidad de resultados (0 o null = sin límite).
    Task<List<Turno>> BuscarAsync(
        int? profesionalId,
        DateOnly? fecha,
        EstadoTurno? estado,
        DateOnly? fechaDesde = null,
        int? tomar = null,
        int? pacienteId = null,
        CancellationToken cancellationToken = default);

    // Verifica si ya existe otro turno activo (no cancelado) para ese profesional,
    // fecha y horario. "idExcluir" permite ignorar el propio turno al validar una edición.
    // Es la verificación optimista de aplicación; la restricción final e infalible
    // ante condiciones de carrera es el índice único parcial en base de datos.
    Task<bool> ExisteSolapamientoAsync(
        int profesionalId,
        DateOnly fecha,
        TimeOnly horario,
        int? idExcluir,
        CancellationToken cancellationToken = default);

    Task<Dictionary<EstadoTurno, int>> ContarPorEstadoAsync(int? profesionalId, int? pacienteId = null, CancellationToken cancellationToken = default);

    // Horarios ya ocupados (turnos activos, no cancelados) de un profesional
    // en una fecha puntual. Usado para calcular la grilla de horarios libres.
    // "idExcluir" permite no contar el propio turno al reprogramar/editar
    // (si no, su propio horario actual aparecería como "ocupado" por sí mismo).
    Task<List<TimeOnly>> ObtenerHorariosOcupadosAsync(int profesionalId, DateOnly fecha, int? idExcluir = null, CancellationToken cancellationToken = default);

    Task AgregarAsync(Turno turno, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default);
}
