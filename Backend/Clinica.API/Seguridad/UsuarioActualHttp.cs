using System.Security.Claims;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Enumeraciones;

namespace Clinica.API.Seguridad;

// Implementación web de IUsuarioActual: lee los claims del token JWT ya validado
// por el middleware de autenticación. Es el único lugar de la capa API que
// traduce HttpContext.User a algo que la capa de Aplicación puede consumir.
public class UsuarioActualHttp : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActualHttp(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UsuarioId =>
        int.TryParse(ObtenerClaim(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public RolUsuario Rol =>
        Enum.TryParse<RolUsuario>(ObtenerClaim(ClaimTypes.Role), out var rol) ? rol : default;

    public int? ProfesionalId =>
        int.TryParse(ObtenerClaim("profesionalId"), out var id) ? id : null;

    private string? ObtenerClaim(string tipo) =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(tipo);
}
