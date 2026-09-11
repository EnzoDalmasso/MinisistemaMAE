using Clinica.Aplicacion.Comun;
using Clinica.Aplicacion.DTOs.Profesionales;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class BloqueHorarioDtoValidador : AbstractValidator<BloqueHorarioDto>
{
    public BloqueHorarioDtoValidador()
    {
        RuleFor(b => b.DiaSemana).IsInEnum();

        RuleFor(b => b.HoraInicio)
            .GreaterThanOrEqualTo(HorarioClinica.Apertura)
            .WithMessage($"El horario de inicio no puede ser anterior a las {HorarioClinica.Apertura:HH\\:mm} (horario general de la clínica).");

        RuleFor(b => b.HoraFin)
            .LessThanOrEqualTo(HorarioClinica.Cierre)
            .WithMessage($"El horario de fin no puede ser posterior a las {HorarioClinica.Cierre:HH\\:mm} (horario general de la clínica).")
            .GreaterThan(b => b.HoraInicio)
            .WithMessage("El horario de fin debe ser posterior al de inicio.");
    }
}
