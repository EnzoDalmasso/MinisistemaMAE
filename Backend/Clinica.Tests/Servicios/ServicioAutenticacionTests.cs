using Clinica.Aplicacion.DTOs.Autenticacion;
using Clinica.Aplicacion.Servicios;
using Clinica.Aplicacion.Validadores;
using Clinica.Infraestructura.Configuracion;
using Clinica.Infraestructura.Persistencia.Contexto;
using Clinica.Infraestructura.Persistencia.Repositorios;
using Clinica.Infraestructura.Seguridad;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Clinica.Tests.Servicios;

public class ServicioAutenticacionTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<ClinicaDbContext> _opciones;

    public ServicioAutenticacionTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<ClinicaDbContext>().UseSqlite(_conexion).Options;

        using var contexto = new ClinicaDbContext(_opciones);
        contexto.Database.EnsureCreated();
    }

    public void Dispose() => _conexion.Dispose();

    private ClinicaDbContext CrearContexto() => new(_opciones);

    private static ServicioAutenticacion CrearServicio(ClinicaDbContext contexto)
    {
        var configuracionJwt = Options.Create(new ConfiguracionJwt
        {
            Clave = "clave-de-prueba-que-no-se-usa-en-produccion-0123456789",
            Emisor = "ClinicaAPI.Tests",
            Audiencia = "ClinicaFrontend.Tests",
            ExpiracionMinutos = 60
        });

        return new ServicioAutenticacion(
            new RepositorioUsuarios(contexto),
            new RepositorioPacientes(contexto),
            new HasheadorContrasenasBCrypt(),
            new GeneradorTokensJwt(configuracionJwt),
            new IniciarSesionDtoValidador(),
            new AccesoPacienteDtoValidador());
    }

    // 11. La primera vez que un DNI accede, se crea el paciente y su usuario.
    [Fact]
    public async Task AccederComoPacienteAsync_ConDniNuevo_CreaPacienteYUsuario()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        var respuesta = await servicio.AccederComoPacienteAsync(
            new AccesoPacienteDto { Nombre = "Camila", Apellido = "Suárez", Dni = "30123456" },
            CancellationToken.None);

        Assert.Equal("Paciente", respuesta.Rol);
        Assert.NotNull(respuesta.PacienteId);
        Assert.NotEmpty(respuesta.Token);

        Assert.Equal(1, await contexto.Pacientes.CountAsync());
        Assert.Equal(1, await contexto.Usuarios.CountAsync());
    }

    // 11 (continuación). Un segundo acceso con el mismo DNI reutiliza la
    // cuenta existente: no duplica ni el paciente ni el usuario.
    [Fact]
    public async Task AccederComoPacienteAsync_ConDniExistente_ReutilizaLaCuenta()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        var primerAcceso = await servicio.AccederComoPacienteAsync(
            new AccesoPacienteDto { Nombre = "Camila", Apellido = "Suárez", Dni = "30123456" },
            CancellationToken.None);

        var segundoAcceso = await servicio.AccederComoPacienteAsync(
            new AccesoPacienteDto { Nombre = "Camila", Apellido = "Suárez", Dni = "30123456" },
            CancellationToken.None);

        Assert.Equal(primerAcceso.PacienteId, segundoAcceso.PacienteId);
        Assert.Equal(1, await contexto.Pacientes.CountAsync());
        Assert.Equal(1, await contexto.Usuarios.CountAsync());
    }
}
