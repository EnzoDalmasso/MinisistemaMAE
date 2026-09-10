using Clinica.Aplicacion.DTOs.Autenticacion;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioAutenticacion
{
    Task<RespuestaAutenticacionDto> IniciarSesionAsync(IniciarSesionDto dto, CancellationToken cancellationToken = default);
}
