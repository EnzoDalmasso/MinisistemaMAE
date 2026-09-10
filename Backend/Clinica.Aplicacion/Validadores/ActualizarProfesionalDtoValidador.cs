using Clinica.Aplicacion.DTOs.Profesionales;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ActualizarProfesionalDtoValidador : AbstractValidator<ActualizarProfesionalDto>
{
    public ActualizarProfesionalDtoValidador()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(p => p.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(p => p.Especialidad)
            .NotEmpty().WithMessage("La especialidad es obligatoria.")
            .MaximumLength(100).WithMessage("La especialidad no puede superar los 100 caracteres.");
    }
}
