namespace Clinica.Aplicacion.DTOs.Turnos;

// Usado por el paciente para reprogramar su propio turno: a diferencia de
// ActualizarTurnoDto, no permite cambiar Paciente, Profesional ni Estado.
public class ReprogramarTurnoDto
{
    public DateOnly Fecha { get; set; }
    public TimeOnly Horario { get; set; }
}
