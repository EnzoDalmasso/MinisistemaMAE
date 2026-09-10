using Clinica.Aplicacion.DTOs.Autenticacion;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Extensiones;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;
using Clinica.Dominio.Interfaces;
using FluentValidation;

namespace Clinica.Aplicacion.Servicios;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IRepositorioPacientes _repositorioPacientes;
    private readonly IHasheadorContrasenas _hasheador;
    private readonly IGeneradorTokens _generadorTokens;
    private readonly IValidator<IniciarSesionDto> _validadorIniciarSesion;
    private readonly IValidator<AccesoPacienteDto> _validadorAccesoPaciente;

    public ServicioAutenticacion(
        IRepositorioUsuarios repositorioUsuarios,
        IRepositorioPacientes repositorioPacientes,
        IHasheadorContrasenas hasheador,
        IGeneradorTokens generadorTokens,
        IValidator<IniciarSesionDto> validadorIniciarSesion,
        IValidator<AccesoPacienteDto> validadorAccesoPaciente)
    {
        _repositorioUsuarios = repositorioUsuarios;
        _repositorioPacientes = repositorioPacientes;
        _hasheador = hasheador;
        _generadorTokens = generadorTokens;
        _validadorIniciarSesion = validadorIniciarSesion;
        _validadorAccesoPaciente = validadorAccesoPaciente;
    }

    public async Task<RespuestaAutenticacionDto> IniciarSesionAsync(IniciarSesionDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorIniciarSesion.ValidarYLanzarAsync(dto, cancellationToken);

        var usuario = await _repositorioUsuarios.ObtenerPorNombreUsuarioAsync(dto.NombreUsuario.Trim(), cancellationToken);

        // Mensaje idéntico tanto si el usuario no existe como si la contraseña es incorrecta,
        // para no revelar qué nombres de usuario están registrados en el sistema.
        if (usuario is null || !usuario.Activo || !_hasheador.Verificar(dto.Contrasena, usuario.ContrasenaHash))
        {
            throw new ExcepcionCredencialesInvalidas();
        }

        return ConstruirRespuesta(usuario);
    }

    public async Task<RespuestaAutenticacionDto> AccederComoPacienteAsync(AccesoPacienteDto dto, CancellationToken cancellationToken = default)
    {
        await _validadorAccesoPaciente.ValidarYLanzarAsync(dto, cancellationToken);

        var dni = dto.Dni.Trim();
        var usuario = await _repositorioUsuarios.ObtenerPorNombreUsuarioAsync(dni, cancellationToken);

        if (usuario is not null && usuario.Rol != RolUsuario.Paciente)
        {
            // Ese "nombre de usuario" (el DNI) ya está tomado por una cuenta
            // de otro rol; no debería pasar en la práctica, pero se cubre
            // para no pisar ni exponer una cuenta ajena.
            throw new ExcepcionCredencialesInvalidas();
        }

        if (usuario is null)
        {
            // Primera vez que este DNI accede: se da de alta el paciente y su
            // usuario en el mismo paso (ver DOCUMENTACION_IA / README:
            // decisión explícita del cliente de no pedir contraseña acá).
            var paciente = new Paciente
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Dni = dni,
                FechaCreacion = DateTime.UtcNow
            };
            await _repositorioPacientes.AgregarAsync(paciente, cancellationToken);

            usuario = new Usuario
            {
                NombreUsuario = dni,
                ContrasenaHash = _hasheador.Hashear(dni),
                Rol = RolUsuario.Paciente,
                PacienteId = paciente.Id,
                FechaCreacion = DateTime.UtcNow
            };
            await _repositorioUsuarios.AgregarAsync(usuario, cancellationToken);
            usuario.Paciente = paciente;
        }

        return ConstruirRespuesta(usuario);
    }

    private (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario) => _generadorTokens.Generar(usuario);

    private RespuestaAutenticacionDto ConstruirRespuesta(Usuario usuario)
    {
        var (token, expiraEn) = GenerarToken(usuario);

        return new RespuestaAutenticacionDto
        {
            Token = token,
            ExpiraEn = expiraEn,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol.ToString(),
            ProfesionalId = usuario.ProfesionalId,
            PacienteId = usuario.PacienteId,
            NombreCompleto = usuario.Paciente is not null ? $"{usuario.Paciente.Nombre} {usuario.Paciente.Apellido}" : null
        };
    }
}
