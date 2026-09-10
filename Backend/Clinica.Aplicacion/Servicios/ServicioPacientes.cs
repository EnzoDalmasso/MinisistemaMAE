using Clinica.Aplicacion.DTOs.Pacientes;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Extensiones;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Interfaces;
using FluentValidation;

namespace Clinica.Aplicacion.Servicios;

public class ServicioPacientes : IServicioPacientes
{
    private readonly IRepositorioPacientes _repositorio;
    private readonly IValidator<CrearPacienteDto> _validadorCrear;
    private readonly IValidator<ActualizarPacienteDto> _validadorActualizar;

    public ServicioPacientes(
        IRepositorioPacientes repositorio,
        IValidator<CrearPacienteDto> validadorCrear,
        IValidator<ActualizarPacienteDto> validadorActualizar)
    {
        _repositorio = repositorio;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
    }

    public async Task<List<PacienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var pacientes = await _repositorio.ObtenerTodosAsync(cancellationToken);
        return pacientes.Select(ADto).ToList();
    }

    public async Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorCrear.ValidarYLanzarAsync(dto, cancellationToken);

        var paciente = new Paciente
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Telefono = dto.Telefono.Trim(),
            ObraSocial = dto.ObraSocial.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        await _repositorio.AgregarAsync(paciente, cancellationToken);
        return ADto(paciente);
    }

    public async Task<PacienteDto> ActualizarAsync(int id, ActualizarPacienteDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorActualizar.ValidarYLanzarAsync(dto, cancellationToken);

        var paciente = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el paciente solicitado.");

        paciente.Nombre = dto.Nombre.Trim();
        paciente.Apellido = dto.Apellido.Trim();
        paciente.Telefono = dto.Telefono.Trim();
        paciente.ObraSocial = dto.ObraSocial.Trim();

        await _repositorio.ActualizarAsync(paciente, cancellationToken);
        return ADto(paciente);
    }

    private static PacienteDto ADto(Paciente p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Telefono = p.Telefono,
        ObraSocial = p.ObraSocial,
        Dni = p.Dni
    };
}
