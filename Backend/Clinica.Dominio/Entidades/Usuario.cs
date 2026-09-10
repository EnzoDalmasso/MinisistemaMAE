using Clinica.Dominio.Enumeraciones;

namespace Clinica.Dominio.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }

    // Solo se completa cuando Rol == Profesional. Vincula el login con su
    // registro de Profesional para que el backend pueda filtrar "sus" turnos
    // usando el claim del token, sin confiar en ningún dato enviado por el cliente.
    public int? ProfesionalId { get; set; }
    public Profesional? Profesional { get; set; }

    // Análogo a ProfesionalId, pero para Rol == Paciente.
    public int? PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
}
