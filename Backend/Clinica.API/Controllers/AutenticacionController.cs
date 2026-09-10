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

    // Login sin contraseña del paciente: crea la cuenta la primera vez que
    // ese DNI accede, o ingresa a la existente (ver ServicioAutenticacion).
    [HttpPost("acceso-paciente")]
    [AllowAnonymous]
    public async Task<ActionResult<RespuestaAutenticacionDto>> AccederComoPaciente(
        [FromBody] AccesoPacienteDto dto,
        CancellationToken cancellationToken)
    {
        var respuesta = await _servicio.AccederComoPacienteAsync(dto, cancellationToken);
        return Ok(respuesta);
    }
}
