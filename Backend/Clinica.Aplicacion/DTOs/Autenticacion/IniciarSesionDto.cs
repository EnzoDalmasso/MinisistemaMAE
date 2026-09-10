namespace Clinica.Aplicacion.DTOs.Autenticacion;

public class IniciarSesionDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}
