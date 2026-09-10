namespace Clinica.Aplicacion.DTOs.Profesionales;

public class ProfesionalDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public int DuracionTurnoMinutos { get; set; }
}
