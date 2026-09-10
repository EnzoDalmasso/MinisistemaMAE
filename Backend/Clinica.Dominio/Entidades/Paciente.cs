namespace Clinica.Dominio.Entidades;

public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
