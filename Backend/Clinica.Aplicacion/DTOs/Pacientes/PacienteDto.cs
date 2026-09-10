namespace Clinica.Aplicacion.DTOs.Pacientes;

public class PacienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    // Nullable: un paciente autogestionado (alta por DNI, sin pasar por el
    // Administrador) no los completa hasta pedir su primer turno (ver
    // ActualizarContactoPacienteDto).
    public string? Telefono { get; set; }
    public string? ObraSocial { get; set; }
    public string? Email { get; set; }

    // Solo tiene valor para pacientes autogestionados.
    public string? Dni { get; set; }
}
