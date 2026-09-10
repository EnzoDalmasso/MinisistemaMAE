using Clinica.Aplicacion.DTOs.Turnos;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class CambiarEstadoTurnoDtoValidador : AbstractValidator<CambiarEstadoTurnoDto>
{
    public CambiarEstadoTurnoDtoValidador()
    {
        RuleFor(t => t.Estado).IsInEnum().WithMessage("El estado del turno no es válido.");
    }
}
