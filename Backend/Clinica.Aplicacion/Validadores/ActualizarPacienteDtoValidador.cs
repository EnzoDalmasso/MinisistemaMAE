using Clinica.Aplicacion.DTOs.Pacientes;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ActualizarPacienteDtoValidador : AbstractValidator<ActualizarPacienteDto>
{
    public ActualizarPacienteDtoValidador()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(p => p.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(p => p.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(30).WithMessage("El teléfono no puede superar los 30 caracteres.")
            .Matches(@"^[0-9+()\-\s]+$").WithMessage("El teléfono contiene caracteres inválidos.");

        RuleFor(p => p.ObraSocial)
            .NotEmpty().WithMessage("La obra social es obligatoria.")
            .MaximumLength(100).WithMessage("La obra social no puede superar los 100 caracteres.");

        RuleFor(p => p.Dni)
            .Matches(@"^\d{7,8}$").WithMessage("El DNI debe tener entre 7 y 8 dígitos, sin puntos ni espacios.")
            .When(p => !string.IsNullOrWhiteSpace(p.Dni));

        RuleFor(p => p.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El email no puede superar los 150 caracteres.")
            .When(p => !string.IsNullOrWhiteSpace(p.Email));
    }
}
