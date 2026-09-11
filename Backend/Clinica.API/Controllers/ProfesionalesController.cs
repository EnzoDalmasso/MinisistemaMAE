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

    // El propio profesional puede cambiar su duración de turno; el
    // administrador, la de cualquiera (ServicioProfesionales valida el dueño).
    [HttpPatch("{id:int}/duracion-turno")]
    [Authorize(Roles = "Administrador,Profesional")]
    public async Task<ActionResult<ProfesionalDto>> ActualizarDuracionTurno(int id, [FromBody] ActualizarDuracionTurnoDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarDuracionTurnoAsync(id, dto, cancellationToken));
    }

    // El propio profesional puede configurar sus días/horarios de atención; el
    // administrador, los de cualquiera (mismo esquema que duracion-turno).
    [HttpPut("{id:int}/horarios")]
    [Authorize(Roles = "Administrador,Profesional")]
    public async Task<ActionResult<ProfesionalDto>> ActualizarHorarios(int id, [FromBody] ActualizarHorariosDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarHorariosAsync(id, dto, cancellationToken));
    }

    [HttpPatch("{id:int}/desactivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProfesionalDto>> Desactivar(int id, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.DesactivarAsync(id, cancellationToken));
    }

    [HttpPatch("{id:int}/reactivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProfesionalDto>> Reactivar(int id, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ReactivarAsync(id, cancellationToken));
    }

    // Definitivo: ServicioProfesionales exige que esté desactivado hace 7+
    // días y sin turnos asociados; si no, devuelve 409 con el motivo.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _servicio.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
