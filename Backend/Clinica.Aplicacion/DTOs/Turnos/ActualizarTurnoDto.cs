using Clinica.Dominio.Enumeraciones;

namespace Clinica.Aplicacion.DTOs.Turnos;

public class ActualizarTurnoDto
{
    public int PacienteId { get; set; }
    public int ProfesionalId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Horario { get; set; }

    // Tipado como enum (no string libre): System.Text.Json con JsonStringEnumConverter
    // rechaza automáticamente cualquier valor que no sea uno de los 4 estados válidos,
    // devolviendo 400 antes de que el request llegue al controlador.
    public EstadoTurno Estado { get; set; }
}
