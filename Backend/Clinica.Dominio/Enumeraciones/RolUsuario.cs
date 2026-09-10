namespace Clinica.Dominio.Enumeraciones;

// Los roles soportados por el sistema. Se define como enum (no como tabla
// "Rol" aparte) porque el conjunto es fijo y no se espera que crezca mucho:
// agregar una tabla de roles sería sobreingeniería para este alcance.
public enum RolUsuario
{
    Administrador = 1,
    Profesional = 2,

    // Se autogestiona (ver ServicioAutenticacion.AccederComoPacienteAsync):
    // no lo crea el administrador, se da de alta solo la primera vez que
    // el paciente pide un turno.
    Paciente = 3
}
