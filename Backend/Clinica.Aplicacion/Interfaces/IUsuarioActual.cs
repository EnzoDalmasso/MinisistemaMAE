using Clinica.Dominio.Enumeraciones;

namespace Clinica.Aplicacion.Interfaces;

// Abstrae el acceso a los claims del usuario autenticado (HttpContext.User).
// Se define en Aplicación e implementa en la capa web (Clinica.API), para que
// los servicios de negocio no dependan directamente de ASP.NET Core y sean testeables.
public interface IUsuarioActual
{
    int UsuarioId { get; }
    RolUsuario Rol { get; }
    int? ProfesionalId { get; }
}
