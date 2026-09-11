using Clinica.Aplicacion.DTOs.Pacientes;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

// La gestión completa de pacientes es exclusiva de Administrador, aplicada
// acción por acción (no a nivel de controlador): un [Authorize(Roles=...)]
// de clase se combina con uno de acción exigiendo AMBOS roles a la vez (no
// "pisa" al de clase), lo que dejaría inalcanzable cualquier endpoint de
// autoservicio con un rol distinto. Mismo patrón que TurnosController.
[ApiController]
[Route("api/pacientes")]
[Authorize]
public class PacientesController : ControllerBase
{
    private readonly IServicioPacientes _servicio;

    public PacientesController(IServicioPacientes servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<List<PacienteDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerTodosAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PacienteDto>> Crear([FromBody] CrearPacienteDto dto, CancellationToken cancellationToken)
    {
        var paciente = await _servicio.CrearAsync(dto, cancellationToken);
        return Created($"/api/pacientes/{paciente.Id}", paciente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<PacienteDto>> Actualizar(int id, [FromBody] ActualizarPacienteDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto, cancellationToken));
    }

    // Definitivo: ServicioPacientes lo rechaza con 409 si el paciente tiene
    // turnos asociados.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _servicio.EliminarAsync(id, cancellationToken);
        return NoContent();
    }

    // Autoservicio del paciente. Nunca recibe un id: el servicio siempre
    // opera sobre el paciente de la cuenta autenticada.
    [HttpGet("mi-perfil")]
    [Authorize(Roles = "Paciente")]
    public async Task<ActionResult<PacienteDto>> ObtenerMiPerfil(CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerPropioAsync(cancellationToken));
    }

    [HttpPut("mi-perfil")]
    [Authorize(Roles = "Paciente")]
    public async Task<ActionResult<PacienteDto>> ActualizarMiContacto([FromBody] ActualizarContactoPacienteDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarContactoPropioAsync(dto, cancellationToken));
    }
}
