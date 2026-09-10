using Clinica.Aplicacion.DTOs.Turnos;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

// Lectura habilitada para los 3 roles (el filtrado por dueño ocurre en
// ServicioTurnos, nunca confiando en el rol solo para ocultar UI). La
// escritura está repartida según quién puede tocar qué: el administrador
// gestiona la agenda completa, el paciente su propio turno (crear/
// reprogramar/cancelar) y el profesional el estado de los suyos.
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

    // El administrador crea turnos para cualquier paciente; el paciente solo
    // puede pedir turno para sí mismo (ServicioTurnos ignora el PacienteId
    // del body en ese caso y usa el de su propia sesión).
    [HttpPost]
    [Authorize(Roles = "Administrador,Paciente")]
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
    [Authorize(Roles = "Administrador,Paciente")]
    public async Task<ActionResult<TurnoDto>> Cancelar(int id, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.CancelarAsync(id, cancellationToken));
    }

    // El paciente reprograma su propio turno (fecha/horario únicamente); no
    // puede cambiar de paciente, profesional ni estado por esta vía.
    [HttpPut("{id:int}/reprogramar")]
    [Authorize(Roles = "Paciente")]
    public async Task<ActionResult<TurnoDto>> Reprogramar(int id, [FromBody] ReprogramarTurnoDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ReprogramarAsync(id, dto, cancellationToken));
    }

    // El profesional marca Confirmado/Atendido/Cancelado en sus propios
    // turnos; el administrador puede usarlo sobre cualquiera.
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = "Administrador,Profesional")]
    public async Task<ActionResult<TurnoDto>> CambiarEstado(int id, [FromBody] CambiarEstadoTurnoDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.CambiarEstadoAsync(id, dto, cancellationToken));
    }
}
