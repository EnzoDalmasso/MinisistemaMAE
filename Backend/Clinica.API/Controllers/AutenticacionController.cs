using Clinica.Aplicacion.DTOs.Autenticacion;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/autenticacion")]
public class AutenticacionController : ControllerBase
{
    private readonly IServicioAutenticacion _servicio;

    public AutenticacionController(IServicioAutenticacion servicio)
    {
        _servicio = servicio;
    }

    [HttpPost("iniciar-sesion")]
    [AllowAnonymous]
    public async Task<ActionResult<RespuestaAutenticacionDto>> IniciarSesion(
        [FromBody] IniciarSesionDto dto,
        CancellationToken cancellationToken)
    {
        var respuesta = await _servicio.IniciarSesionAsync(dto, cancellationToken);
        return Ok(respuesta);
    }
}
