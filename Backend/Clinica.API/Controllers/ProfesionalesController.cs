using Clinica.Aplicacion.DTOs.Profesionales;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/profesionales")]
[Authorize(Roles = "Administrador")]
public class ProfesionalesController : ControllerBase
{
    private readonly IServicioProfesionales _servicio;

    public ProfesionalesController(IServicioProfesionales servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProfesionalDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerTodosAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ProfesionalDto>> Crear([FromBody] CrearProfesionalDto dto, CancellationToken cancellationToken)
    {
        var profesional = await _servicio.CrearAsync(dto, cancellationToken);
        return Created($"/api/profesionales/{profesional.Id}", profesional);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProfesionalDto>> Actualizar(int id, [FromBody] ActualizarProfesionalDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto, cancellationToken));
    }
}
