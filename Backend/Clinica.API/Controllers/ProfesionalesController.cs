using Clinica.Aplicacion.DTOs.Profesionales;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/profesionales")]
[Authorize]
public class ProfesionalesController : ControllerBase
{
    private readonly IServicioProfesionales _servicio;

    public ProfesionalesController(IServicioProfesionales servicio)
    {
        _servicio = servicio;
    }

    // Cualquier rol autenticado puede listar profesionales: no expone datos
    // sensibles (solo nombre/especialidad) y el paciente lo necesita para
    // elegir con quién pedir su turno.
    [HttpGet]
    public async Task<ActionResult<List<ProfesionalDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerTodosAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProfesionalDto>> Crear([FromBody] CrearProfesionalDto dto, CancellationToken cancellationToken)
    {
        var profesional = await _servicio.CrearAsync(dto, cancellationToken);
        return Created($"/api/profesionales/{profesional.Id}", profesional);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProfesionalDto>> Actualizar(int id, [FromBody] ActualizarProfesionalDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto, cancellationToken));
    }
}
