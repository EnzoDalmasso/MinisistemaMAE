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
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IRepositorioTurnos _repositorioTurnos;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IValidator<CrearPacienteDto> _validadorCrear;
    private readonly IValidator<ActualizarPacienteDto> _validadorActualizar;
    private readonly IValidator<ActualizarContactoPacienteDto> _validadorContacto;

    public ServicioPacientes(
        IRepositorioPacientes repositorio,
        IRepositorioUsuarios repositorioUsuarios,
        IRepositorioTurnos repositorioTurnos,
        IUsuarioActual usuarioActual,
        IValidator<CrearPacienteDto> validadorCrear,
        IValidator<ActualizarPacienteDto> validadorActualizar,
        IValidator<ActualizarContactoPacienteDto> validadorContacto)
    {
        _repositorio = repositorio;
        _repositorioUsuarios = repositorioUsuarios;
        _repositorioTurnos = repositorioTurnos;
        _usuarioActual = usuarioActual;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
        _validadorContacto = validadorContacto;
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

        var dni = string.IsNullOrWhiteSpace(dto.Dni) ? null : dto.Dni.Trim();
        if (dni is not null)
        {
            var otroConEseDni = await _repositorio.ObtenerPorDniAsync(dni, cancellationToken);
            if (otroConEseDni is not null && otroConEseDni.Id != id)
            {
                throw new ExcepcionConflicto("Ese DNI ya está en uso por otro paciente.");
            }
        }

        paciente.Nombre = dto.Nombre.Trim();
        paciente.Apellido = dto.Apellido.Trim();
        paciente.Telefono = dto.Telefono.Trim();
        paciente.ObraSocial = dto.ObraSocial.Trim();
        paciente.Dni = dni;

        await _repositorio.ActualizarAsync(paciente, cancellationToken);
        return ADto(paciente);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var paciente = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el paciente solicitado.");

        // Nunca se rompe el historial de turnos: si tuvo aunque sea uno
        // (incluido uno cancelado), se bloquea la eliminación definitiva.
        if (await _repositorioTurnos.ExisteAlgunoPorPacienteAsync(id, cancellationToken))
        {
            throw new ExcepcionConflicto(
                "No se puede eliminar un paciente con turnos asociados (se perdería ese historial).");
        }

        // Si se autogestionó, tiene un Usuario vinculado que hay que borrar
        // primero (la FK es Restrict).
        var usuario = await _repositorioUsuarios.ObtenerPorPacienteIdAsync(id, cancellationToken);
        if (usuario is not null)
        {
            await _repositorioUsuarios.EliminarAsync(usuario, cancellationToken);
        }

        await _repositorio.EliminarAsync(paciente, cancellationToken);
    }

    public async Task<PacienteDto> ObtenerPropioAsync(CancellationToken cancellationToken = default)
    {
        var paciente = await _repositorio.ObtenerPorIdAsync(ObtenerPacienteIdPropio(), cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el paciente asociado a la cuenta.");
        return ADto(paciente);
    }

    public async Task<PacienteDto> ActualizarContactoPropioAsync(ActualizarContactoPacienteDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorContacto.ValidarYLanzarAsync(dto, cancellationToken);

        var paciente = await _repositorio.ObtenerPorIdAsync(ObtenerPacienteIdPropio(), cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el paciente asociado a la cuenta.");

        paciente.Telefono = dto.Telefono.Trim();
        paciente.ObraSocial = dto.ObraSocial.Trim();
        paciente.Email = dto.Email.Trim();

        await _repositorio.ActualizarAsync(paciente, cancellationToken);
        return ADto(paciente);
    }

    // Nunca confía en un id que venga del cliente: el autoservicio siempre
    // opera sobre el paciente vinculado a la cuenta autenticada.
    private int ObtenerPacienteIdPropio() =>
        _usuarioActual.PacienteId ?? throw new ExcepcionProhibido("La cuenta no tiene un paciente asociado.");

    private static PacienteDto ADto(Paciente p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Telefono = p.Telefono,
        ObraSocial = p.ObraSocial,
        Email = p.Email,
        Dni = p.Dni
    };
}
