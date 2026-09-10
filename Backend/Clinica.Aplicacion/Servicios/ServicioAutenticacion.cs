using Clinica.Aplicacion.DTOs.Autenticacion;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Extensiones;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Interfaces;
using FluentValidation;

namespace Clinica.Aplicacion.Servicios;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IHasheadorContrasenas _hasheador;
    private readonly IGeneradorTokens _generadorTokens;
    private readonly IValidator<IniciarSesionDto> _validador;

    public ServicioAutenticacion(
        IRepositorioUsuarios repositorioUsuarios,
        IHasheadorContrasenas hasheador,
        IGeneradorTokens generadorTokens,
        IValidator<IniciarSesionDto> validador)
    {
        _repositorioUsuarios = repositorioUsuarios;
        _hasheador = hasheador;
        _generadorTokens = generadorTokens;
        _validador = validador;
    }

    public async Task<RespuestaAutenticacionDto> IniciarSesionAsync(IniciarSesionDto dto, CancellationToken cancellationToken = default)
    {
        await _validador.ValidarYLanzarAsync(dto, cancellationToken);

        var usuario = await _repositorioUsuarios.ObtenerPorNombreUsuarioAsync(dto.NombreUsuario.Trim(), cancellationToken);

        // Mensaje idéntico tanto si el usuario no existe como si la contraseña es incorrecta,
        // para no revelar qué nombres de usuario están registrados en el sistema.
        if (usuario is null || !usuario.Activo || !_hasheador.Verificar(dto.Contrasena, usuario.ContrasenaHash))
        {
            throw new ExcepcionCredencialesInvalidas();
        }

        var (token, expiraEn) = _generadorTokens.Generar(usuario);

        return new RespuestaAutenticacionDto
        {
            Token = token,
            ExpiraEn = expiraEn,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol.ToString(),
            ProfesionalId = usuario.ProfesionalId
        };
    }
}
