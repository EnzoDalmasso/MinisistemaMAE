namespace Clinica.Aplicacion.DTOs.Profesionales;

public class ActualizarProfesionalDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public int DuracionTurnoMinutos { get; set; } = 30;
}
