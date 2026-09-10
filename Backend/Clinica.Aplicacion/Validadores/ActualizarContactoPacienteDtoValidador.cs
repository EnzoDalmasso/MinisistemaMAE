using Clinica.Aplicacion.DTOs.Pacientes;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ActualizarContactoPacienteDtoValidador : AbstractValidator<ActualizarContactoPacienteDto>
{
    public ActualizarContactoPacienteDtoValidador()
    {
        RuleFor(p => p.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(30).WithMessage("El teléfono no puede superar los 30 caracteres.")
            .Matches(@"^[0-9+()\-\s]+$").WithMessage("El teléfono contiene caracteres inválidos.");

        RuleFor(p => p.ObraSocial)
            .NotEmpty().WithMessage("La obra social es obligatoria.")
            .MaximumLength(100).WithMessage("La obra social no puede superar los 100 caracteres.");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El email no puede superar los 150 caracteres.");
    }
}
