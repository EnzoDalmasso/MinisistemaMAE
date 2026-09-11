namespace Clinica.Aplicacion.DTOs.Profesionales;

public class BloqueHorarioDto
{
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
}
