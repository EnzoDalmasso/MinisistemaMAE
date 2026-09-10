using Clinica.Aplicacion.DTOs.Turnos;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ReprogramarTurnoDtoValidador : AbstractValidator<ReprogramarTurnoDto>
{
    private static readonly TimeOnly HorarioApertura = new(7, 0);
    private static readonly TimeOnly HorarioCierre = new(21, 0);

    public ReprogramarTurnoDtoValidador()
    {
        // Igual que al crear un turno nuevo: no tiene sentido reprogramar a
        // una fecha que ya pasó.
        RuleFor(t => t.Fecha)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("La fecha del turno no puede ser anterior a hoy.");

        RuleFor(t => t.Horario)
            .InclusiveBetween(HorarioApertura, HorarioCierre)
            .WithMessage($"El horario debe estar entre las {HorarioApertura:HH\\:mm} y las {HorarioCierre:HH\\:mm}.");
    }
}
