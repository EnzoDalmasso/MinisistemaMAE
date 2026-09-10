namespace Clinica.Aplicacion.DTOs.Profesionales;

public class CrearProfesionalDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;

    // Cada cuántos minutos se ofrece un horario de este profesional al pedir
    // turno. 30 por defecto si no se especifica.
    public int DuracionTurnoMinutos { get; set; } = 30;
}
