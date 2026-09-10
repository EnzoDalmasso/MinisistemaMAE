namespace Clinica.Dominio.Entidades;

public class Profesional
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;

    // Cuánto dura cada turno de este profesional: define la grilla de
    // horarios que se ofrece al pedir/reprogramar un turno (ver
    // ServicioTurnos.ObtenerHorariosDisponiblesAsync). Lo puede modificar el
    // propio profesional o el administrador.
    public int DuracionTurnoMinutos { get; set; } = 30;

    public DateTime FechaCreacion { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
