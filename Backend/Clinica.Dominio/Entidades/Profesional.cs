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

    // Email de contacto (distinto del NombreUsuario con el que loguea el
    // profesional en su Usuario vinculado). Opcional: no se pide al crear.
    public string? Email { get; set; }

    // Un profesional desactivado no puede loguearse (se desactiva también su
    // Usuario vinculado) ni se lo ofrece para pedir turnos nuevos, pero
    // conserva su historial de turnos. Ver ServicioProfesionales.
    public bool Activo { get; set; } = true;
    public DateTime? FechaDesactivacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();

    // Días y horarios en los que atiende. Vacío significa "todavía no
    // configuró un horario propio": ServicioTurnos cae en ese caso al
    // horario general de la clínica (ver HorarioClinica), para no romper a
    // los profesionales existentes que no lo hayan cargado.
    public ICollection<BloqueHorarioProfesional> BloquesHorario { get; set; } = new List<BloqueHorarioProfesional>();
}
