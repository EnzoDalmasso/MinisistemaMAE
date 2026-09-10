namespace Clinica.Aplicacion.DTOs.Profesionales;

public class ActualizarProfesionalDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public int DuracionTurnoMinutos { get; set; } = 30;

    public string? Email { get; set; }

    // Opcional: si viene vacío/null, la contraseña actual no se toca. Se usa
    // para que el administrador pueda resetearle la contraseña al
    // profesional sin necesidad de un endpoint aparte.
    public string? NuevaContrasena { get; set; }
}
