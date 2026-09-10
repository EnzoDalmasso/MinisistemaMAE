namespace Clinica.Aplicacion.DTOs.Autenticacion;

public class RespuestaAutenticacionDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? ProfesionalId { get; set; }
}
