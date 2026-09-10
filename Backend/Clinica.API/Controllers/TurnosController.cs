using Clinica.Aplicacion.DTOs.Turnos;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

// Lectura habilitada para ambos roles (el filtrado por dueño ocurre en
// ServicioTurnos, nunca confiando en el rol solo para ocultar UI). Escritura
// restringida a Administrador, que es quien gestiona la agenda completa.
[ApiController]
[Route("api/turnos")]
[Authorize]
public class TurnosController : ControllerBase
{
    private readonly IServicioTurnos _servicio;

    public TurnosController(IServicioTurnos servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<TurnoDto>>> Obtener([FromQuery] TurnoFiltroDto filtro, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerAsync(filtro, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TurnoDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerPorIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<TurnoDto>> Crear([FromBody] CrearTurnoDto dto, CancellationToken cancellationToken)
    {
        var turno = await _servicio.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = turno.Id }, turno);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<TurnoDto>> Actualizar(int id, [FromBody] ActualizarTurnoDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto, cancellationToken));
    }

    [HttpPatch("{id:int}/cancelar")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<TurnoDto>> Cancelar(int id, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.CancelarAsync(id, cancellationToken));
    }
}
