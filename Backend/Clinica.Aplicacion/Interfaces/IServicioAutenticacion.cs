using Clinica.Aplicacion.DTOs.Autenticacion;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioAutenticacion
{
    Task<RespuestaAutenticacionDto> IniciarSesionAsync(IniciarSesionDto dto, CancellationToken cancellationToken = default);

    // Login sin contraseña del paciente: crea la cuenta si es la primera vez
    // que ese DNI accede, o ingresa a la existente.
    Task<RespuestaAutenticacionDto> AccederComoPacienteAsync(AccesoPacienteDto dto, CancellationToken cancellationToken = default);
}
