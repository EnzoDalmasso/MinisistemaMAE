using Clinica.Dominio.Enumeraciones;

namespace Clinica.Aplicacion.DTOs.Turnos;

// DTO "aplanado": trae los datos de paciente/profesional necesarios para la grilla
// sin anidar objetos completos, para mantener el payload liviano.
public class TurnoDto
{
    public int Id { get; set; }

    public int PacienteId { get; set; }
    public string PacienteNombreCompleto { get; set; } = string.Empty;

    public int ProfesionalId { get; set; }
    public string ProfesionalNombreCompleto { get; set; } = string.Empty;
    public string ProfesionalEspecialidad { get; set; } = string.Empty;

    public DateOnly Fecha { get; set; }
    public TimeOnly Horario { get; set; }
    public EstadoTurno Estado { get; set; }

    public DateTime FechaCreacion { get; set; }
}
