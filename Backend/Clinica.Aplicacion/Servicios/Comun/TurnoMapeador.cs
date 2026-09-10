using Clinica.Aplicacion.DTOs.Turnos;
using Clinica.Dominio.Entidades;

namespace Clinica.Aplicacion.Servicios.Comun;

// Mapeo manual (sin AutoMapper): con 4 entidades en el dominio, una dependencia
// extra para esto no se justifica y este mapeo explícito es más fácil de seguir.
internal static class TurnoMapeador
{
    public static TurnoDto ADto(Turno turno) => new()
    {
        Id = turno.Id,
        PacienteId = turno.PacienteId,
        PacienteNombreCompleto = $"{turno.Paciente.Nombre} {turno.Paciente.Apellido}",
        ProfesionalId = turno.ProfesionalId,
        ProfesionalNombreCompleto = $"{turno.Profesional.Nombre} {turno.Profesional.Apellido}",
        ProfesionalEspecialidad = turno.Profesional.Especialidad,
        Fecha = turno.Fecha,
        Horario = turno.Horario,
        Estado = turno.Estado,
        FechaCreacion = turno.FechaCreacion
    };
}
