using Clinica.Aplicacion.DTOs.Turnos;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class CrearTurnoDtoValidador : AbstractValidator<CrearTurnoDto>
{
    // Horario de atención de la clínica. Es una validación simple a nivel global,
    // no un sistema de horarios por profesional (fuera de alcance, ver mejoras futuras).
    private static readonly TimeOnly HorarioApertura = new(7, 0);
    private static readonly TimeOnly HorarioCierre = new(21, 0);

    public CrearTurnoDtoValidador()
    {
        RuleFor(t => t.PacienteId)
            .GreaterThan(0).WithMessage("Debe seleccionar un paciente.");

        RuleFor(t => t.ProfesionalId)
            .GreaterThan(0).WithMessage("Debe seleccionar un profesional.");

        // No se permite agendar turnos nuevos en fechas pasadas.
        RuleFor(t => t.Fecha)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("La fecha del turno no puede ser anterior a hoy.");

        RuleFor(t => t.Horario)
            .InclusiveBetween(HorarioApertura, HorarioCierre)
            .WithMessage($"El horario debe estar entre las {HorarioApertura:HH\\:mm} y las {HorarioCierre:HH\\:mm}.");
    }
}
