using Clinica.Aplicacion.DTOs.Turnos;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Extensiones;
using Clinica.Aplicacion.Interfaces;
using Clinica.Aplicacion.Servicios.Comun;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;
using Clinica.Dominio.Interfaces;
using FluentValidation;

namespace Clinica.Aplicacion.Servicios;

public class ServicioTurnos : IServicioTurnos
{
    private const string MensajeConflictoDisponibilidad =
        "El profesional ya tiene un turno asignado para esa fecha y horario.";

    private readonly IRepositorioTurnos _repositorioTurnos;
    private readonly IRepositorioPacientes _repositorioPacientes;
    private readonly IRepositorioProfesionales _repositorioProfesionales;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IValidator<CrearTurnoDto> _validadorCrear;
    private readonly IValidator<ActualizarTurnoDto> _validadorActualizar;

    public ServicioTurnos(
        IRepositorioTurnos repositorioTurnos,
        IRepositorioPacientes repositorioPacientes,
        IRepositorioProfesionales repositorioProfesionales,
        IUsuarioActual usuarioActual,
        IValidator<CrearTurnoDto> validadorCrear,
        IValidator<ActualizarTurnoDto> validadorActualizar)
    {
        _repositorioTurnos = repositorioTurnos;
        _repositorioPacientes = repositorioPacientes;
        _repositorioProfesionales = repositorioProfesionales;
        _usuarioActual = usuarioActual;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
    }

    public async Task<List<TurnoDto>> ObtenerAsync(TurnoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        // Regla de autorización crítica: si el usuario autenticado es Profesional,
        // se ignora cualquier ProfesionalId que haya llegado en el filtro y se
        // fuerza el propio. Nunca se confía en el query string para esto.
        var profesionalId = _usuarioActual.Rol == RolUsuario.Profesional
            ? _usuarioActual.ProfesionalId
            : filtro.ProfesionalId;

        var turnos = await _repositorioTurnos.BuscarAsync(profesionalId, filtro.Fecha, filtro.Estado, cancellationToken: cancellationToken);
        return turnos.Select(TurnoMapeador.ADto).ToList();
    }

    public async Task<TurnoDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var turno = await _repositorioTurnos.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el turno solicitado.");

        VerificarPertenencia(turno);
        return TurnoMapeador.ADto(turno);
    }

    public async Task<TurnoDto> CrearAsync(CrearTurnoDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorCrear.ValidarYLanzarAsync(dto, cancellationToken);
        await VerificarExistenciaAsync(dto.PacienteId, dto.ProfesionalId, cancellationToken);
        await VerificarDisponibilidadAsync(dto.ProfesionalId, dto.Fecha, dto.Horario, idExcluir: null, cancellationToken);

        var turno = new Turno
        {
            PacienteId = dto.PacienteId,
            ProfesionalId = dto.ProfesionalId,
            Fecha = dto.Fecha,
            Horario = dto.Horario,
            Estado = EstadoTurno.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        // AgregarAsync puede lanzar ExcepcionConflicto si, pese a la verificación
        // anterior, el índice único de la base de datos rechaza el insert por una
        // condición de carrera (ver RepositorioTurnos en Clinica.Infraestructura).
        await _repositorioTurnos.AgregarAsync(turno, cancellationToken);

        var creado = await _repositorioTurnos.ObtenerPorIdAsync(turno.Id, cancellationToken) ?? turno;
        return TurnoMapeador.ADto(creado);
    }

    public async Task<TurnoDto> ActualizarAsync(int id, ActualizarTurnoDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorActualizar.ValidarYLanzarAsync(dto, cancellationToken);

        var turno = await _repositorioTurnos.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el turno solicitado.");

        await VerificarExistenciaAsync(dto.PacienteId, dto.ProfesionalId, cancellationToken);

        // Un turno que pasa a Cancelado libera el horario, así que no tiene
        // sentido (ni sería correcto) revalidar disponibilidad en ese caso.
        if (dto.Estado != EstadoTurno.Cancelado)
        {
            await VerificarDisponibilidadAsync(dto.ProfesionalId, dto.Fecha, dto.Horario, idExcluir: id, cancellationToken);
        }

        turno.PacienteId = dto.PacienteId;
        turno.ProfesionalId = dto.ProfesionalId;
        turno.Fecha = dto.Fecha;
        turno.Horario = dto.Horario;
        turno.Estado = dto.Estado;
        turno.FechaModificacion = DateTime.UtcNow;

        await _repositorioTurnos.ActualizarAsync(turno, cancellationToken);

        var actualizado = await _repositorioTurnos.ObtenerPorIdAsync(turno.Id, cancellationToken) ?? turno;
        return TurnoMapeador.ADto(actualizado);
    }

    public async Task<TurnoDto> CancelarAsync(int id, CancellationToken cancellationToken = default)
    {
        var turno = await _repositorioTurnos.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el turno solicitado.");

        turno.Estado = EstadoTurno.Cancelado;
        turno.FechaModificacion = DateTime.UtcNow;

        await _repositorioTurnos.ActualizarAsync(turno, cancellationToken);
        return TurnoMapeador.ADto(turno);
    }

    private void VerificarPertenencia(Turno turno)
    {
        if (_usuarioActual.Rol == RolUsuario.Profesional && turno.ProfesionalId != _usuarioActual.ProfesionalId)
        {
            throw new ExcepcionProhibido("No tiene permiso para acceder a este turno.");
        }
    }

    private async Task VerificarExistenciaAsync(int pacienteId, int profesionalId, CancellationToken cancellationToken)
    {
        if (await _repositorioPacientes.ObtenerPorIdAsync(pacienteId, cancellationToken) is null)
        {
            throw new ExcepcionNoEncontrado("No se encontró el paciente seleccionado.");
        }

        if (await _repositorioProfesionales.ObtenerPorIdAsync(profesionalId, cancellationToken) is null)
        {
            throw new ExcepcionNoEncontrado("No se encontró el profesional seleccionado.");
        }
    }

    private async Task VerificarDisponibilidadAsync(int profesionalId, DateOnly fecha, TimeOnly horario, int? idExcluir, CancellationToken cancellationToken)
    {
        var existeSolapamiento = await _repositorioTurnos.ExisteSolapamientoAsync(profesionalId, fecha, horario, idExcluir, cancellationToken);
        if (existeSolapamiento)
        {
            throw new ExcepcionConflicto(MensajeConflictoDisponibilidad);
        }
    }
}
