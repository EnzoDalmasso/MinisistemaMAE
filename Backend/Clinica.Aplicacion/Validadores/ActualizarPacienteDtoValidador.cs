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
    }
}
