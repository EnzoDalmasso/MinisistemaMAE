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
    private readonly IRepositorioProfesionales _repositorio;
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IHasheadorContrasenas _hasheador;
    private readonly IValidator<CrearProfesionalDto> _validadorCrear;
    private readonly IValidator<ActualizarProfesionalDto> _validadorActualizar;
    private readonly IValidator<ActualizarDuracionTurnoDto> _validadorDuracion;

    public ServicioProfesionales(
        IRepositorioProfesionales repositorio,
        IRepositorioUsuarios repositorioUsuarios,
        IUsuarioActual usuarioActual,
        IHasheadorContrasenas hasheador,
        IValidator<CrearProfesionalDto> validadorCrear,
        IValidator<ActualizarProfesionalDto> validadorActualizar,
        IValidator<ActualizarDuracionTurnoDto> validadorDuracion)
    {
        _repositorio = repositorio;
        _repositorioUsuarios = repositorioUsuarios;
        _usuarioActual = usuarioActual;
        _hasheador = hasheador;
        _validadorCrear = validadorCrear;
        _validadorActualizar = validadorActualizar;
        _validadorDuracion = validadorDuracion;
    }

    public async Task<List<ProfesionalDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var profesionales = await _repositorio.ObtenerTodosAsync(cancellationToken);
        return profesionales.Select(ADto).ToList();
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
        profesional.DuracionTurnoMinutos = dto.DuracionTurnoMinutos;

        await _repositorio.ActualizarAsync(profesional, cancellationToken);
        return ADto(profesional);
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
        return ADto(profesional);
    }

    private static ProfesionalDto ADto(Profesional p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Especialidad = p.Especialidad,
        DuracionTurnoMinutos = p.DuracionTurnoMinutos
    };
}
