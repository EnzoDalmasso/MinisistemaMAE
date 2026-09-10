using Clinica.Aplicacion.Comun;
using Clinica.Aplicacion.DTOs.Turnos;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ReprogramarTurnoDtoValidador : AbstractValidator<ReprogramarTurnoDto>
{
    public ReprogramarTurnoDtoValidador()
    {
        // Igual que al crear un turno nuevo: no tiene sentido reprogramar a
        // una fecha que ya pasó.
        RuleFor(t => t.Fecha)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("La fecha del turno no puede ser anterior a hoy.");

        RuleFor(t => t.Horario)
            .InclusiveBetween(HorarioClinica.Apertura, HorarioClinica.Cierre)
            .WithMessage($"El horario debe estar entre las {HorarioClinica.Apertura:HH\\:mm} y las {HorarioClinica.Cierre:HH\\:mm}.")
            .Must((dto, horario) => dto.Fecha != DateOnly.FromDateTime(DateTime.Now) || horario > TimeOnly.FromDateTime(DateTime.Now))
            .WithMessage("No se puede agendar un turno en un horario que ya pasó.");
    }
}
