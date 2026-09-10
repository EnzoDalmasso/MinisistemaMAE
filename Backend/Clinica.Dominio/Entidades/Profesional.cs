namespace Clinica.Dominio.Entidades;

public class Profesional
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
