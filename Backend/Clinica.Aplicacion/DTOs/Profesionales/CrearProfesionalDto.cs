namespace Clinica.Aplicacion.DTOs.Profesionales;

public class CrearProfesionalDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;

    // Cada cuántos minutos se ofrece un horario de este profesional al pedir
    // turno. 30 por defecto si no se especifica.
    public int DuracionTurnoMinutos { get; set; } = 30;

    // Email de contacto, opcional (no forma parte del login).
    public string? Email { get; set; }

    // El administrador define acá las credenciales con las que el
    // profesional va a loguearse (ver ServicioProfesionales.CrearAsync, que
    // da de alta el Usuario vinculado en el mismo paso).
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}
