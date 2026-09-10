namespace Clinica.Aplicacion.DTOs.Pacientes;

public class PacienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
}
