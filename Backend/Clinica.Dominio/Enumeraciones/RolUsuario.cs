namespace Clinica.Dominio.Enumeraciones;

// Los dos únicos roles soportados por el sistema. Se define como enum (no como
// tabla "Rol" aparte) porque el conjunto es fijo y no se espera que crezca:
// agregar una tabla de roles sería sobreingeniería para este alcance.
public enum RolUsuario
{
    Administrador = 1,
    Profesional = 2
}
