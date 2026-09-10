using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Interfaces;
using Clinica.Infraestructura.Persistencia.Contexto;
using Clinica.Infraestructura.Persistencia.Repositorios;
using Clinica.Infraestructura.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Infraestructura.Configuracion;

public static class ExtensionesInyeccionDependencias
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var cadenaConexion = configuracion.GetConnectionString("BaseDeDatos")
            ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'BaseDeDatos'.");

        servicios.AddDbContext<ClinicaDbContext>(opciones => opciones.UseNpgsql(cadenaConexion));

        servicios.Configure<ConfiguracionJwt>(configuracion.GetSection(ConfiguracionJwt.Seccion));

        servicios.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
        servicios.AddScoped<IRepositorioPacientes, RepositorioPacientes>();
        servicios.AddScoped<IRepositorioProfesionales, RepositorioProfesionales>();
        servicios.AddScoped<IRepositorioTurnos, RepositorioTurnos>();

        servicios.AddScoped<IHasheadorContrasenas, HasheadorContrasenasBCrypt>();
        servicios.AddScoped<IGeneradorTokens, GeneradorTokensJwt>();

        return servicios;
    }
}
