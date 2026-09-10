namespace Clinica.Aplicacion.DTOs.Pacientes;

// DTO acotado para el autoservicio del paciente (ver ServicioPacientes.
// ActualizarContactoPropioAsync): a diferencia de ActualizarPacienteDto, de
// uso exclusivo del Administrador, este no permite tocar Nombre/Apellido/Dni.
public class ActualizarContactoPacienteDto
{
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
