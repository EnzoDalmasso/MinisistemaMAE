using Clinica.Aplicacion.DTOs.Autenticacion;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class AccesoPacienteDtoValidador : AbstractValidator<AccesoPacienteDto>
{
    public AccesoPacienteDtoValidador()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(p => p.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(p => p.Dni)
            .NotEmpty().WithMessage("El DNI es obligatorio.")
            .Matches(@"^\d{7,8}$").WithMessage("El DNI debe tener entre 7 y 8 dígitos, sin puntos ni espacios.");
    }
}
