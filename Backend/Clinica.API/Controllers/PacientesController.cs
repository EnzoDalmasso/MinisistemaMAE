using Clinica.Aplicacion.DTOs.Pacientes;
using Clinica.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.API.Controllers;

// Toda la gestión de pacientes es exclusiva del rol Administrador
// se aplica a nivel de controlador para no tener que repetirlo en cada acción.
[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = "Administrador")]
public class PacientesController : ControllerBase
{
    private readonly IServicioPacientes _servicio;

    public PacientesController(IServicioPacientes servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<PacienteDto>>> ObtenerTodos(CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ObtenerTodosAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PacienteDto>> Crear([FromBody] CrearPacienteDto dto, CancellationToken cancellationToken)
    {
        var paciente = await _servicio.CrearAsync(dto, cancellationToken);
        return Created($"/api/pacientes/{paciente.Id}", paciente);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PacienteDto>> Actualizar(int id, [FromBody] ActualizarPacienteDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _servicio.ActualizarAsync(id, dto, cancellationToken));
    }
}
