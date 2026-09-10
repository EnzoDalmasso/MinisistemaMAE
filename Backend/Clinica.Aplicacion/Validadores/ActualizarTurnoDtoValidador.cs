using Clinica.Aplicacion.DTOs.Turnos;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ActualizarTurnoDtoValidador : AbstractValidator<ActualizarTurnoDto>
{
    private static readonly TimeOnly HorarioApertura = new(7, 0);
    private static readonly TimeOnly HorarioCierre = new(21, 0);

    public ActualizarTurnoDtoValidador()
    {
        RuleFor(t => t.PacienteId)
            .GreaterThan(0).WithMessage("Debe seleccionar un paciente.");

        RuleFor(t => t.ProfesionalId)
            .GreaterThan(0).WithMessage("Debe seleccionar un profesional.");

        // A diferencia de la creación, no se exige que la fecha sea futura: debe
        // poder editarse (por ejemplo, marcar como "Atendido") un turno cuya fecha
        // ya pasó, sin verse bloqueado por esta regla.
        RuleFor(t => t.Horario)
            .InclusiveBetween(HorarioApertura, HorarioCierre)
            .WithMessage($"El horario debe estar entre las {HorarioApertura:HH\\:mm} y las {HorarioCierre:HH\\:mm}.");

        RuleFor(t => t.Estado)
            .IsInEnum().WithMessage("El estado del turno no es válido.");
    }
}
