using Clinica.Aplicacion.DTOs.Profesionales;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Servicios;
using Clinica.Aplicacion.Validadores;
using Clinica.Dominio.Enumeraciones;
using Clinica.Infraestructura.Persistencia.Contexto;
using Clinica.Infraestructura.Persistencia.Repositorios;
using Clinica.Infraestructura.Seguridad;
using Clinica.Tests.Comunes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinica.Tests.Servicios;

public class ServicioProfesionalesTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<ClinicaDbContext> _opciones;

    public ServicioProfesionalesTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<ClinicaDbContext>().UseSqlite(_conexion).Options;

        using var contexto = new ClinicaDbContext(_opciones);
        contexto.Database.EnsureCreated();
    }

    public void Dispose() => _conexion.Dispose();

    private ClinicaDbContext CrearContexto() => new(_opciones);

    private static ServicioProfesionales CrearServicio(ClinicaDbContext contexto) => new(
        new RepositorioProfesionales(contexto),
        new RepositorioUsuarios(contexto),
        new UsuarioActualFalso(RolUsuario.Administrador),
        new HasheadorContrasenasBCrypt(),
        new CrearProfesionalDtoValidador(),
        new ActualizarProfesionalDtoValidador(),
        new ActualizarDuracionTurnoDtoValidador());

    // Sin esto, un profesional cargado por el administrador quedaba en el
    // sistema pero sin ninguna forma de loguearse y ver su propia agenda.
    [Fact]
    public async Task CrearAsync_CreaElUsuarioDeLoginVinculadoAlProfesional()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        var dto = new CrearProfesionalDto
        {
            Nombre = "Martín",
            Apellido = "Pereyra",
            Especialidad = "Pediatría",
            DuracionTurnoMinutos = 30,
            NombreUsuario = "martin.pereyra",
            Contrasena = "ClaveSegura123"
        };

        var profesional = await servicio.CrearAsync(dto, CancellationToken.None);

        var usuario = await contexto.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == "martin.pereyra");
        Assert.NotNull(usuario);
        Assert.Equal(RolUsuario.Profesional, usuario!.Rol);
        Assert.Equal(profesional.Id, usuario.ProfesionalId);
        Assert.NotEqual("ClaveSegura123", usuario.ContrasenaHash); // nunca en texto plano
    }

    [Fact]
    public async Task CrearAsync_ConNombreDeUsuarioYaTomado_LanzaConflicto()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);

        var dto = new CrearProfesionalDto
        {
            Nombre = "Martín", Apellido = "Pereyra", Especialidad = "Pediatría",
            NombreUsuario = "repetido", Contrasena = "ClaveSegura123"
        };
        await servicio.CrearAsync(dto, CancellationToken.None);

        var dtoRepetido = new CrearProfesionalDto
        {
            Nombre = "Carla", Apellido = "Sosa", Especialidad = "Dermatología",
            NombreUsuario = "repetido", Contrasena = "OtraClave456"
        };

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.CrearAsync(dtoRepetido, CancellationToken.None));
    }
}
