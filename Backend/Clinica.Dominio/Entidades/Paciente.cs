namespace Clinica.Dominio.Entidades;

public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    // Solo se completa cuando el paciente se autogestionó (ver
    // ServicioAutenticacion.AccederComoPacienteAsync); un paciente cargado
    // manualmente por el Administrador no tiene login propio y queda null.
    public string? Dni { get; set; }

    // Nullable porque el alta autogestionada del paciente (solo Nombre +
    // Apellido + DNI) no los pide; el alta manual del Administrador sí los
    // sigue exigiendo a través de su propio validador.
    public string? Telefono { get; set; }
    public string? ObraSocial { get; set; }
    public string? Email { get; set; }

    public DateTime FechaCreacion { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
