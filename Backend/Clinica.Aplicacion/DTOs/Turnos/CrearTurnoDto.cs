namespace Clinica.Aplicacion.DTOs.Turnos;

// No incluye Estado a propósito: todo turno nuevo arranca en "Pendiente".
// Esa regla la fija el servicio, no el cliente.
public class CrearTurnoDto
{
    public int PacienteId { get; set; }
    public int ProfesionalId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Horario { get; set; }
}
