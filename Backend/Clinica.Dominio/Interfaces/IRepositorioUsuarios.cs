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

    // Usado para cambiar la contraseña o (des)activar el login de un
    // profesional desde el administrador (ver ServicioProfesionales).
    Task<Usuario?> ObtenerPorProfesionalIdAsync(int profesionalId, CancellationToken cancellationToken = default);

    Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);

    // Solo se usa al eliminar definitivamente un profesional: su Usuario
    // vinculado tiene que borrarse antes (la FK es Restrict).
    Task EliminarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
