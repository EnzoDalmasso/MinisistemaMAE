namespace Clinica.Aplicacion.DTOs.Autenticacion;

public class RespuestaAutenticacionDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? ProfesionalId { get; set; }
    public int? PacienteId { get; set; }

    // Solo para Paciente: NombreUsuario es su DNI, que no es agradable de
    // mostrar en la interfaz — el frontend usa este campo en su lugar.
    public string? NombreCompleto { get; set; }
}
