namespace Clinica.Aplicacion.DTOs.Turnos;

// No incluye Estado a propósito: lo fija el servicio, no el cliente (ver
// ServicioTurnos.CrearAsync — Pendiente si lo carga el Administrador,
// Confirmado si lo pide el propio paciente).
public class CrearTurnoDto
{
    public int PacienteId { get; set; }
    public int ProfesionalId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Horario { get; set; }
}
