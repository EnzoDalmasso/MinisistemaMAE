using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioUsuarios
{
    // Incluye el Profesional relacionado para poder armar el claim ProfesionalId del token.
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
}
