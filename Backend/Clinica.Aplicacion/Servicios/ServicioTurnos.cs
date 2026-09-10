using Clinica.Aplicacion.Comun;
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
    private readonly IValidator<ReprogramarTurnoDto> _validadorReprogramar;
    private readonly IValidator<CambiarEstadoTurnoDto> _validadorCambiarEstado;

    public ServicioTurnos(
        IRepositorioTurnos repositorioTurnos,
        IRepositorioPacientes repositorioPacientes,
        IRepositorioProfesionales repositorioProfesionales,
        IUsuarioActual usuarioActual,
        IValidator<CrearTurnoDto> validadorCrear,
        IValidator<ActualizarTurnoDto> validadorActualizar,
        IValidator<ReprogramarTurnoDto> validadorReprogramar,
        IValidator<CambiarEstadoTurnoDto> validadorCambiarEstado)
    {
        _repositorioTurnos = repositorioTurnos;
        _repositorioPacientes = repositorioPacientes;
        _repositorioProfesionales = repositorioProfesionales;
        _usuarioActual = usuarioActual;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
        _validadorReprogramar = validadorReprogramar;
        _validadorCambiarEstado = validadorCambiarEstado;
    }

    public async Task<List<TurnoDto>> ObtenerAsync(TurnoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        // Regla de autorización crítica: si el usuario autenticado es Profesional
        // o Paciente, se ignora cualquier filtro que haya llegado y se fuerza el
        // propio ProfesionalId/PacienteId. Nunca se confía en el query string para esto.
        var profesionalId = _usuarioActual.Rol == RolUsuario.Profesional
            ? _usuarioActual.ProfesionalId
            : filtro.ProfesionalId;

        var pacienteId = _usuarioActual.Rol == RolUsuario.Paciente
            ? _usuarioActual.PacienteId
            : null;

        var turnos = await _repositorioTurnos.BuscarAsync(
            profesionalId, filtro.Fecha, filtro.Estado, pacienteId: pacienteId, cancellationToken: cancellationToken);
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

        // Si es un paciente autogestionándose, el turno siempre es para sí
        // mismo: se ignora cualquier PacienteId que haya llegado en el body.
        var pacienteId = _usuarioActual.Rol == RolUsuario.Paciente
            ? ObtenerPacienteIdPropio()
            : dto.PacienteId;

        await VerificarExistenciaAsync(pacienteId, dto.ProfesionalId, cancellationToken);
        await VerificarDisponibilidadAsync(dto.ProfesionalId, dto.Fecha, dto.Horario, idExcluir: null, cancellationToken);

        var turno = new Turno
        {
            PacienteId = pacienteId,
            ProfesionalId = dto.ProfesionalId,
            Fecha = dto.Fecha,
            Horario = dto.Horario,
            // El paciente elige de una grilla que ya muestra disponibilidad
            // real (ver ObtenerHorariosDisponiblesAsync), así que su turno
            // queda Confirmado directamente: no hay ningún paso manual
            // pendiente. El turno que carga el Administrador sí arranca en
            // Pendiente, porque puede necesitar coordinarlo con el paciente.
            Estado = _usuarioActual.Rol == RolUsuario.Paciente ? EstadoTurno.Confirmado : EstadoTurno.Pendiente,
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

        // Permitido para Administrador (cualquier turno) y Paciente (solo el
        // propio); el controlador ya bloquea a Profesional en este endpoint.
        VerificarPertenencia(turno);

        turno.Estado = EstadoTurno.Cancelado;
        turno.FechaModificacion = DateTime.UtcNow;

        await _repositorioTurnos.ActualizarAsync(turno, cancellationToken);
        return TurnoMapeador.ADto(turno);
    }

    public async Task<TurnoDto> ReprogramarAsync(int id, ReprogramarTurnoDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorReprogramar.ValidarYLanzarAsync(dto, cancellationToken);

        var turno = await _repositorioTurnos.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el turno solicitado.");

        VerificarPertenencia(turno);

        if (turno.Estado is EstadoTurno.Cancelado or EstadoTurno.Atendido)
        {
            throw new ExcepcionConflicto("No se puede reprogramar un turno cancelado o ya atendido.");
        }

        await VerificarDisponibilidadAsync(turno.ProfesionalId, dto.Fecha, dto.Horario, idExcluir: id, cancellationToken);

        turno.Fecha = dto.Fecha;
        turno.Horario = dto.Horario;
        turno.FechaModificacion = DateTime.UtcNow;

        await _repositorioTurnos.ActualizarAsync(turno, cancellationToken);

        var actualizado = await _repositorioTurnos.ObtenerPorIdAsync(turno.Id, cancellationToken) ?? turno;
        return TurnoMapeador.ADto(actualizado);
    }

    public async Task<TurnoDto> CambiarEstadoAsync(int id, CambiarEstadoTurnoDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorCambiarEstado.ValidarYLanzarAsync(dto, cancellationToken);

        var turno = await _repositorioTurnos.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el turno solicitado.");

        if (_usuarioActual.Rol == RolUsuario.Profesional)
        {
            if (turno.ProfesionalId != _usuarioActual.ProfesionalId)
            {
                throw new ExcepcionProhibido("No tiene permiso para modificar este turno.");
            }

            // El profesional solo registra qué pasó con la cita (se atendió o
            // no); los estados previos (Pendiente/Confirmado) son
            // administrativos y quedan reservados al Administrador.
            if (dto.Estado is not (EstadoTurno.Atendido or EstadoTurno.Cancelado))
            {
                throw new ExcepcionProhibido("Solo puede marcar el turno como Atendido o Cancelado.");
            }
        }

        if (dto.Estado != EstadoTurno.Cancelado)
        {
            await VerificarDisponibilidadAsync(turno.ProfesionalId, turno.Fecha, turno.Horario, idExcluir: id, cancellationToken);
        }

        turno.Estado = dto.Estado;
        turno.FechaModificacion = DateTime.UtcNow;

        await _repositorioTurnos.ActualizarAsync(turno, cancellationToken);

        var actualizado = await _repositorioTurnos.ObtenerPorIdAsync(turno.Id, cancellationToken) ?? turno;
        return TurnoMapeador.ADto(actualizado);
    }

    public async Task<List<TimeOnly>> ObtenerHorariosDisponiblesAsync(int profesionalId, DateOnly fecha, int? idExcluirTurno = null, CancellationToken cancellationToken = default)
    {
        var profesional = await _repositorioProfesionales.ObtenerPorIdAsync(profesionalId, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional seleccionado.");

        var ocupados = await _repositorioTurnos.ObtenerHorariosOcupadosAsync(profesionalId, fecha, idExcluirTurno, cancellationToken);
        var ocupadosPorHorario = ocupados.ToHashSet();

        var duracion = TimeSpan.FromMinutes(profesional.DuracionTurnoMinutos);
        var disponibles = new List<TimeOnly>();

        // Si la fecha consultada es hoy, además de los ocupados hay que
        // descartar los horarios que ya pasaron (si no, se seguían ofreciendo
        // turnos "disponibles" a las 09:00 de un día que ya está a las 16:00).
        var ahora = DateTime.Now;
        var horarioMinimo = fecha == DateOnly.FromDateTime(ahora) ? TimeOnly.FromDateTime(ahora) : TimeOnly.MinValue;

        // Genera la grilla completa (apertura -> cierre, de a "duracion") y
        // descarta los horarios que ya tienen un turno activo o que ya
        // pasaron. El último horario ofrecido es el último que termina sin
        // pasarse del cierre.
        var horarioActual = HorarioClinica.Apertura;
        while (horarioActual.Add(duracion) <= HorarioClinica.Cierre)
        {
            if (horarioActual > horarioMinimo && !ocupadosPorHorario.Contains(horarioActual))
            {
                disponibles.Add(horarioActual);
            }
            horarioActual = horarioActual.Add(duracion);
        }

        return disponibles;
    }

    private void VerificarPertenencia(Turno turno)
    {
        if (_usuarioActual.Rol == RolUsuario.Profesional && turno.ProfesionalId != _usuarioActual.ProfesionalId)
        {
            throw new ExcepcionProhibido("No tiene permiso para acceder a este turno.");
        }

        if (_usuarioActual.Rol == RolUsuario.Paciente && turno.PacienteId != _usuarioActual.PacienteId)
        {
            throw new ExcepcionProhibido("No tiene permiso para acceder a este turno.");
        }
    }

    private int ObtenerPacienteIdPropio() =>
        _usuarioActual.PacienteId ?? throw new ExcepcionProhibido("La cuenta no tiene un paciente asociado.");

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
