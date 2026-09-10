using Clinica.Aplicacion.DTOs.Panel;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/panel")]
[Authorize]
public class PanelController : ControllerBase
{
    private readonly IServicioPanel _servicio;

    public PanelController(IServicioPanel servicio)
    {
        _servicio = servicio;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenPanelDto>> ObtenerResumen(CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerResumenAsync(cancellationToken));
    }
}
