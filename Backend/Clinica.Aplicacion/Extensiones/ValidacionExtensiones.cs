using Clinica.Aplicacion.Excepciones;
using FluentValidation;

namespace Clinica.Aplicacion.Extensiones;

internal static class ValidacionExtensiones
{
    // Punto único usado por todos los servicios para validar un DTO de entrada
    // y transformar los errores de FluentValidation en ExcepcionValidacion,
    // evitando repetir el mismo bloque if/throw en cada servicio.
    public static async Task ValidarYLanzarAsync<T>(this IValidator<T> validador, T instancia, CancellationToken cancellationToken = default)
    {
        var resultado = await validador.ValidateAsync(instancia, cancellationToken);
        if (!resultado.IsValid)
        {
            var errores = resultado.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ExcepcionValidacion(errores);
        }
    }
}
