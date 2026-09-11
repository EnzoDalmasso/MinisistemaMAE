namespace Clinica.Aplicacion.DTOs.Pacientes;

public class CrearPacienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;

    // Opcional: el alta manual del Administrador no siempre lo tiene a mano.
    public string? Email { get; set; }
}
