using Clinica.Aplicacion.DTOs.Profesionales;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ActualizarDuracionTurnoDtoValidador : AbstractValidator<ActualizarDuracionTurnoDto>
{
    public ActualizarDuracionTurnoDtoValidador()
    {
        RuleFor(p => p.DuracionTurnoMinutos)
            .InclusiveBetween(5, 180).WithMessage("La duración del turno debe estar entre 5 y 180 minutos.")
            .Must(valor => valor % 5 == 0).WithMessage("La duración del turno debe ser múltiplo de 5 minutos.");
    }
}
