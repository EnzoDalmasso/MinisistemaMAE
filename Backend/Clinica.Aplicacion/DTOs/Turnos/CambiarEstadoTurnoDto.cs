using Clinica.Dominio.Enumeraciones;

namespace Clinica.Aplicacion.DTOs.Turnos;

// Usado por el profesional (y el administrador) para cambiar solo el estado
// de un turno, sin tocar paciente/profesional/fecha/horario.
public class CambiarEstadoTurnoDto
{
    public EstadoTurno Estado { get; set; }
}
