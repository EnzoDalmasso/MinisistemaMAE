using Clinica.Aplicacion.DTOs.Profesionales;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Extensiones;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;
using Clinica.Dominio.Interfaces;
using FluentValidation;

namespace Clinica.Aplicacion.Servicios;

public class ServicioProfesionales : IServicioProfesionales
{
    // Un profesional desactivado recién puede eliminarse definitivamente
    // pasado este plazo, para dar margen a revertir una desactivación
    // accidental antes de que la baja sea irreversible.
    private static readonly TimeSpan PlazoMinimoParaEliminar = TimeSpan.FromDays(7);

    private readonly IRepositorioProfesionales _repositorio;
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IRepositorioTurnos _repositorioTurnos;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IHasheadorContrasenas _hasheador;
    private readonly IValidator<CrearProfesionalDto> _validadorCrear;
    private readonly IValidator<ActualizarProfesionalDto> _validadorActualizar;
    private readonly IValidator<ActualizarDuracionTurnoDto> _validadorDuracion;

    public ServicioProfesionales(
        IRepositorioProfesionales repositorio,
        IRepositorioUsuarios repositorioUsuarios,
        IRepositorioTurnos repositorioTurnos,
        IUsuarioActual usuarioActual,
        IHasheadorContrasenas hasheador,
        IValidator<CrearProfesionalDto> validadorCrear,
        IValidator<ActualizarProfesionalDto> validadorActualizar,
        IValidator<ActualizarDuracionTurnoDto> validadorDuracion)
    {
        _repositorio = repositorio;
        _repositorioUsuarios = repositorioUsuarios;
        _repositorioTurnos = repositorioTurnos;
        _usuarioActual = usuarioActual;
        _hasheador = hasheador;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
        _validadorDuracion = validadorDuracion;
    }

    public async Task<List<ProfesionalDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var profesionales = await _repositorio.ObtenerTodosAsync(cancellationToken);

        // Una sola consulta para saber qué profesionales tienen turnos, en
        // vez de una por fila, para poder calcular "PuedeEliminarse" de todos.
        var profesionalesConTurnos = await _repositorioTurnos.ObtenerProfesionalesConTurnosAsync(cancellationToken);

        return profesionales.Select(p => ADto(p, profesionalesConTurnos.Contains(p.Id))).ToList();
    }

    public async Task<ProfesionalDto> CrearAsync(CrearProfesionalDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorCrear.ValidarYLanzarAsync(dto, cancellationToken);

        var nombreUsuario = dto.NombreUsuario.Trim();
        if (await _repositorioUsuarios.ObtenerPorNombreUsuarioAsync(nombreUsuario, cancellationToken) is not null)
        {
            throw new ExcepcionConflicto("Ese nombre de usuario ya está en uso.");
        }

        var profesional = new Profesional
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Especialidad = dto.Especialidad.Trim(),
            DuracionTurnoMinutos = dto.DuracionTurnoMinutos,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        await _repositorio.AgregarAsync(profesional, cancellationToken);

        // Se crea en el mismo paso el usuario con el que el profesional va a
        // loguearse y ver su propia agenda; sin esto, quedaba cargado en el
        // sistema pero sin forma de acceder.
        await _repositorioUsuarios.AgregarAsync(new Usuario
        {
            NombreUsuario = nombreUsuario,
            ContrasenaHash = _hasheador.Hashear(dto.Contrasena),
            Rol = RolUsuario.Profesional,
            ProfesionalId = profesional.Id,
            FechaCreacion = DateTime.UtcNow
        }, cancellationToken);

        return ADto(profesional, tieneTurnos: false);
    }

    public async Task<ProfesionalDto> ActualizarAsync(int id, ActualizarProfesionalDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorActualizar.ValidarYLanzarAsync(dto, cancellationToken);

        var profesional = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional solicitado.");

        profesional.Nombre = dto.Nombre.Trim();
        profesional.Apellido = dto.Apellido.Trim();
        profesional.Especialidad = dto.Especialidad.Trim();
        profesional.DuracionTurnoMinutos = dto.DuracionTurnoMinutos;
        profesional.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

        await _repositorio.ActualizarAsync(profesional, cancellationToken);

        // Cambio de contraseña opcional: si no vino NuevaContrasena, no se
        // toca el Usuario vinculado.
        if (!string.IsNullOrEmpty(dto.NuevaContrasena))
        {
            var usuario = await _repositorioUsuarios.ObtenerPorProfesionalIdAsync(id, cancellationToken)
                ?? throw new ExcepcionConflicto("Este profesional no tiene una cuenta de acceso vinculada.");

            usuario.ContrasenaHash = _hasheador.Hashear(dto.NuevaContrasena);
            await _repositorioUsuarios.ActualizarAsync(usuario, cancellationToken);
        }

        return ADto(profesional, await _repositorioTurnos.ExisteAlgunoPorProfesionalAsync(id, cancellationToken));
    }

    public async Task<ProfesionalDto> ActualizarDuracionTurnoAsync(int id, ActualizarDuracionTurnoDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorDuracion.ValidarYLanzarAsync(dto, cancellationToken);

        var profesional = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional solicitado.");

        // El profesional solo puede tocar su propia duración; el administrador,
        // la de cualquiera. El controlador ya exige uno de esos dos roles.
        if (_usuarioActual.Rol == RolUsuario.Profesional && _usuarioActual.ProfesionalId != id)
        {
            throw new ExcepcionProhibido("No tiene permiso para modificar este profesional.");
        }

        profesional.DuracionTurnoMinutos = dto.DuracionTurnoMinutos;

        await _repositorio.ActualizarAsync(profesional, cancellationToken);
        return ADto(profesional, await _repositorioTurnos.ExisteAlgunoPorProfesionalAsync(id, cancellationToken));
    }

    public async Task<ProfesionalDto> DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var profesional = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional solicitado.");

        profesional.Activo = false;
        profesional.FechaDesactivacion = DateTime.UtcNow;
        await _repositorio.ActualizarAsync(profesional, cancellationToken);

        // También se desactiva su Usuario para que no pueda seguir logueado;
        // sin esto, un profesional desactivado igual podría entrar y ver su agenda.
        await DesactivarUsuarioVinculadoAsync(id, activo: false, cancellationToken);

        return ADto(profesional, await _repositorioTurnos.ExisteAlgunoPorProfesionalAsync(id, cancellationToken));
    }

    public async Task<ProfesionalDto> ReactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        var profesional = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional solicitado.");

        profesional.Activo = true;
        profesional.FechaDesactivacion = null;
        await _repositorio.ActualizarAsync(profesional, cancellationToken);

        await DesactivarUsuarioVinculadoAsync(id, activo: true, cancellationToken);

        return ADto(profesional, await _repositorioTurnos.ExisteAlgunoPorProfesionalAsync(id, cancellationToken));
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var profesional = await _repositorio.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new ExcepcionNoEncontrado("No se encontró el profesional solicitado.");

        if (profesional.Activo || profesional.FechaDesactivacion is null ||
            DateTime.UtcNow - profesional.FechaDesactivacion.Value < PlazoMinimoParaEliminar)
        {
            throw new ExcepcionConflicto(
                "Solo se puede eliminar un profesional desactivado hace al menos 7 días. Mientras tanto, puede dejarlo desactivado.");
        }

        // Nunca se rompe el historial de turnos: si tuvo aunque sea uno
        // (incluido uno cancelado), se bloquea la eliminación definitiva.
        if (await _repositorioTurnos.ExisteAlgunoPorProfesionalAsync(id, cancellationToken))
        {
            throw new ExcepcionConflicto(
                "No se puede eliminar un profesional con turnos asociados (se perdería ese historial). Puede dejarlo desactivado.");
        }

        // El Usuario vinculado se borra primero: la FK Usuario -> Profesional es Restrict.
        var usuario = await _repositorioUsuarios.ObtenerPorProfesionalIdAsync(id, cancellationToken);
        if (usuario is not null)
        {
            await _repositorioUsuarios.EliminarAsync(usuario, cancellationToken);
        }

        await _repositorio.EliminarAsync(profesional, cancellationToken);
    }

    private async Task DesactivarUsuarioVinculadoAsync(int profesionalId, bool activo, CancellationToken cancellationToken)
    {
        var usuario = await _repositorioUsuarios.ObtenerPorProfesionalIdAsync(profesionalId, cancellationToken);
        if (usuario is null)
        {
            return;
        }

        usuario.Activo = activo;
        await _repositorioUsuarios.ActualizarAsync(usuario, cancellationToken);
    }

    private static ProfesionalDto ADto(Profesional p, bool tieneTurnos) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Especialidad = p.Especialidad,
        DuracionTurnoMinutos = p.DuracionTurnoMinutos,
        Email = p.Email,
        Activo = p.Activo,
        PuedeEliminarse = !p.Activo && !tieneTurnos && p.FechaDesactivacion is not null &&
            DateTime.UtcNow - p.FechaDesactivacion.Value >= PlazoMinimoParaEliminar
    };
}
