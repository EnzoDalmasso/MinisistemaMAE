using Clinica.Aplicacion.DTOs.Autenticacion;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class IniciarSesionDtoValidador : AbstractValidator<IniciarSesionDto>
{
    public IniciarSesionDtoValidador()
    {
        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(50).WithMessage("El usuario no puede superar los 50 caracteres.");

        RuleFor(x => x.Contrasena)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MaximumLength(100).WithMessage("La contraseña no puede superar los 100 caracteres.");
    }
}
