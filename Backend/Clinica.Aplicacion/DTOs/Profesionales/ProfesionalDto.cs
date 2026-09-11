namespace Clinica.Aplicacion.DTOs.Profesionales;

public class ProfesionalDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public int DuracionTurnoMinutos { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }

    // Calculado por el servicio: desactivado hace al menos 7 días y sin
    // ningún turno asociado. El frontend lo usa para habilitar/ocultar el
    // botón "Eliminar" sin reimplementar la regla.
    public bool PuedeEliminarse { get; set; }

    // Días y horarios en los que atiende. Vacío significa "sin horario
    // propio configurado" (ver ServicioTurnos.ObtenerHorariosDisponiblesAsync).
    public List<BloqueHorarioDto> Horarios { get; set; } = new();
}
