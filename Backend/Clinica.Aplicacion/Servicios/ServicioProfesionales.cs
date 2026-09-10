using Clinica.Aplicacion.DTOs.Profesionales;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Extensiones;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Interfaces;
using FluentValidation;

namespace Clinica.Aplicacion.Servicios;

public class ServicioProfesionales : IServicioProfesionales
{
    private readonly IRepositorioProfesionales _repositorio;
    private readonly IValidator<CrearProfesionalDto> _validadorCrear;
    private readonly IValidator<ActualizarProfesionalDto> _validadorActualizar;

    public ServicioProfesionales(
        IRepositorioProfesionales repositorio,
        IValidator<CrearProfesionalDto> validadorCrear,
        IValidator<ActualizarProfesionalDto> validadorActualizar)
    {
        _repositorio = repositorio;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
    }

    public async Task<List<ProfesionalDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var profesionales = await _repositorio.ObtenerTodosAsync(cancellationToken);
        return profesionales.Select(ADto).ToList();
    }

    public async Task<ProfesionalDto> CrearAsync(CrearProfesionalDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorCrear.ValidarYLanzarAsync(dto, cancellationToken);

        var profesional = new Profesional
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Especialidad = dto.Especialidad.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        await _repositorio.AgregarAsync(profesional, cancellationToken);
        return ADto(profesional);
    }

    public async Task<ProfesionalDto> ActualizarAsync(int id, ActualizarProfesionalDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorActualizar.ValidarYLanzarAsync(dto, cancellationToken);

        var profesional = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional solicitado.");

        profesional.Nombre = dto.Nombre.Trim();
        profesional.Apellido = dto.Apellido.Trim();
        profesional.Especialidad = dto.Especialidad.Trim();

        await _repositorio.ActualizarAsync(profesional, cancellationToken);
        return ADto(profesional);
    }

    private static ProfesionalDto ADto(Profesional p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Especialidad = p.Especialidad
    };
}
