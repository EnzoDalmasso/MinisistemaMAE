namespace Clinica.Aplicacion.DTOs.Pacientes;

public class ActualizarPacienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;

    // Opcional: permite al Administrador corregir un DNI mal cargado por un
    // paciente autogestionado. Vacío/null limpia el DNI.
    public string? Dni { get; set; }
}
