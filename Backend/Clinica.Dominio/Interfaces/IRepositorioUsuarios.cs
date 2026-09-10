using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioUsuarios
{
    // Incluye el Profesional y el Paciente relacionados para poder armar los
    // claims ProfesionalId/PacienteId del token.
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);

    // Usado al autogestionarse un paciente: crea el usuario asociado (DNI
    // como nombre de usuario) la primera vez que accede.
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
