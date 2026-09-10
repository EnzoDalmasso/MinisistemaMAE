using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Enumeraciones;

namespace Clinica.Tests.Comunes;

// Doble de prueba de IUsuarioActual: permite simular tanto a un administrador
// como a un profesional puntual sin depender de HttpContext ni de un token real.
public class UsuarioActualFalso : IUsuarioActual
{
    public UsuarioActualFalso(RolUsuario rol, int? profesionalId = null, int usuarioId = 1)
    {
        Rol = rol;
        ProfesionalId = profesionalId;
        UsuarioId = usuarioId;
    }

    public int UsuarioId { get; }
    public RolUsuario Rol { get; }
    public int? ProfesionalId { get; }
}
