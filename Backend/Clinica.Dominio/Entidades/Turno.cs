using Clinica.Dominio.Enumeraciones;

namespace Clinica.Dominio.Entidades;

public class Turno
{
    public int Id { get; set; }

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public int ProfesionalId { get; set; }
    public Profesional Profesional { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public TimeOnly Horario { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // No se declara una propiedad de concurrencia explícita: se usa la columna
    // de sistema "xmin" de PostgreSQL como token de concurrencia optimista,
    // configurada como shadow property en TurnoConfiguracion (UseXminAsConcurrencyToken).
}
