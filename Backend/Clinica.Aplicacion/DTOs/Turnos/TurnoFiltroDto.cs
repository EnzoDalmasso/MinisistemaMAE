using Clinica.Dominio.Enumeraciones;

namespace Clinica.Aplicacion.DTOs.Turnos;

// Representa los query params soportados por GET /api/turnos. ProfesionalId
// lo puede enviar el administrador para filtrar; si lo envía un usuario con
// rol Profesional, el servicio lo ignora y usa el propio (ver ServicioTurnos).
public class TurnoFiltroDto
{
    public int? ProfesionalId { get; set; }
    public DateOnly? Fecha { get; set; }
    public EstadoTurno? Estado { get; set; }
}
