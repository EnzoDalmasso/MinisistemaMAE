using Clinica.Aplicacion.DTOs.Profesionales;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class CrearProfesionalDtoValidador : AbstractValidator<CrearProfesionalDto>
{
    public CrearProfesionalDtoValidador()
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

        RuleFor(p => p.DuracionTurnoMinutos)
            .InclusiveBetween(5, 180).WithMessage("La duración del turno debe estar entre 5 y 180 minutos.")
            .Must(valor => valor % 5 == 0).WithMessage("La duración del turno debe ser múltiplo de 5 minutos.");

        RuleFor(p => p.NombreUsuario)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(50).WithMessage("El usuario no puede superar los 50 caracteres.");

        RuleFor(p => p.Contrasena)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .MaximumLength(100).WithMessage("La contraseña no puede superar los 100 caracteres.");
    }
}
