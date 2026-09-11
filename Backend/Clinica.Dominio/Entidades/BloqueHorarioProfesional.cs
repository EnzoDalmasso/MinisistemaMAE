namespace Clinica.Dominio.Entidades;

// Un rango horario en el que un profesional atiende un día de la semana
// puntual (ej: Lunes 08:00-12:00). Un mismo profesional puede tener varios
// bloques el mismo día (ej: Lunes 08:00-12:00 y Lunes 14:00-18:00) para
// representar un corte de mediodía. Ver ServicioTurnos.ObtenerHorariosDisponiblesAsync,
// que arma la grilla de turnos ofrecidos a partir de estos bloques.
public class BloqueHorarioProfesional
{
    public int Id { get; set; }
    public int ProfesionalId { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    public Profesional Profesional { get; set; } = null!;
}
