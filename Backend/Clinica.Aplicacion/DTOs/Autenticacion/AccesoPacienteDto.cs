namespace Clinica.Aplicacion.DTOs.Autenticacion;

// El paciente "accede" con estos 3 datos, sin contraseña: si ya existe una
// cuenta con ese DNI, ingresa a ella; si no, se crea en el momento. Es una
// simplificación deliberada para esta demo (ver README, sección de
// decisiones técnicas / seguridad) — no reemplaza un mecanismo de
// autenticación real para un sistema en producción.
public class AccesoPacienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
}
