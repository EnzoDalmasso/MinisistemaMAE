namespace Clinica.Infraestructura.Configuracion;

// Se puebla desde la sección "Jwt" de configuración (appsettings + variables
// de entorno). La clave nunca se hardcodea: ver appsettings.json / .env.example.
public class ConfiguracionJwt
{
    public const string Seccion = "Jwt";

    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int ExpiracionMinutos { get; set; } = 120;
}
