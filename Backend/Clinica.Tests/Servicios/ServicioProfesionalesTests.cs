using Clinica.Aplicacion.DTOs.Profesionales;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Servicios;
using Clinica.Aplicacion.Validadores;
using Clinica.Dominio.Entidades;
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

    private static ServicioProfesionales CrearServicio(
        ClinicaDbContext contexto, RolUsuario rol = RolUsuario.Administrador, int? profesionalId = null) => new(
        new RepositorioProfesionales(contexto),
        new RepositorioUsuarios(contexto),
        new RepositorioTurnos(contexto),
        new UsuarioActualFalso(rol, profesionalId),
        new HasheadorContrasenasBCrypt(),
        new CrearProfesionalDtoValidador(),
        new ActualizarProfesionalDtoValidador(),
        new ActualizarDuracionTurnoDtoValidador(),
        new ActualizarHorariosDtoValidador());

    private static async Task<Profesional> CrearProfesionalConLoginAsync(
        ClinicaDbContext contexto, ServicioProfesionales servicio, string nombreUsuario = "martin.pereyra")
    {
        var dto = new CrearProfesionalDto
        {
            Nombre = "Martín",
            Apellido = "Pereyra",
            Especialidad = "Pediatría",
            DuracionTurnoMinutos = 30,
            NombreUsuario = nombreUsuario,
            Contrasena = "ClaveSegura123"
        };
        var creado = await servicio.CrearAsync(dto, CancellationToken.None);
        return await contexto.Profesionales.FirstAsync(p => p.Id == creado.Id);
    }

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

    // El administrador puede actualizar el email de contacto y, opcionalmente
    // en el mismo llamado, resetear la contraseña del profesional.
    [Fact]
    public async Task ActualizarAsync_ConEmailYNuevaContrasena_ActualizaAmbosEnElMismoLlamado()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);

        var resultado = await servicio.ActualizarAsync(profesional.Id, new ActualizarProfesionalDto
        {
            Nombre = profesional.Nombre,
            Apellido = profesional.Apellido,
            Especialidad = profesional.Especialidad,
            DuracionTurnoMinutos = profesional.DuracionTurnoMinutos,
            Email = "martin.pereyra@example.com",
            NuevaContrasena = "OtraClaveNueva456"
        }, CancellationToken.None);

        Assert.Equal("martin.pereyra@example.com", resultado.Email);

        var usuario = await contexto.Usuarios.FirstAsync(u => u.ProfesionalId == profesional.Id);
        Assert.True(new HasheadorContrasenasBCrypt().Verificar("OtraClaveNueva456", usuario.ContrasenaHash));
    }

    // Si no mandan NuevaContrasena, la contraseña actual no se toca.
    [Fact]
    public async Task ActualizarAsync_SinNuevaContrasena_NoModificaLaContrasenaActual()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);
        var hashOriginal = (await contexto.Usuarios.FirstAsync(u => u.ProfesionalId == profesional.Id)).ContrasenaHash;

        await servicio.ActualizarAsync(profesional.Id, new ActualizarProfesionalDto
        {
            Nombre = profesional.Nombre,
            Apellido = profesional.Apellido,
            Especialidad = profesional.Especialidad,
            DuracionTurnoMinutos = profesional.DuracionTurnoMinutos
        }, CancellationToken.None);

        var hashDespues = (await contexto.Usuarios.FirstAsync(u => u.ProfesionalId == profesional.Id)).ContrasenaHash;
        Assert.Equal(hashOriginal, hashDespues);
    }

    // Desactivar debe dejar sin acceso al profesional (su Usuario también
    // queda Activo = false), no solo marcarlo en la ficha.
    [Fact]
    public async Task DesactivarAsync_DejaInactivoAlProfesionalYASuUsuarioVinculado()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);

        var resultado = await servicio.DesactivarAsync(profesional.Id, CancellationToken.None);

        Assert.False(resultado.Activo);
        var usuario = await contexto.Usuarios.FirstAsync(u => u.ProfesionalId == profesional.Id);
        Assert.False(usuario.Activo);
    }

    [Fact]
    public async Task ReactivarAsync_RevierteLaDesactivacion()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);
        await servicio.DesactivarAsync(profesional.Id, CancellationToken.None);

        var resultado = await servicio.ReactivarAsync(profesional.Id, CancellationToken.None);

        Assert.True(resultado.Activo);
        var usuario = await contexto.Usuarios.FirstAsync(u => u.ProfesionalId == profesional.Id);
        Assert.True(usuario.Activo);
    }

    [Fact]
    public async Task EliminarAsync_ReciénDesactivado_LanzaConflicto()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);
        await servicio.DesactivarAsync(profesional.Id, CancellationToken.None);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.EliminarAsync(profesional.Id, CancellationToken.None));
    }

    [Fact]
    public async Task EliminarAsync_ConTurnosAsociados_LanzaConflictoAunquePasaronLos7Dias()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);

        var paciente = new Paciente { Nombre = "Julián", Apellido = "Ramírez", Telefono = "11-5555-0000", ObraSocial = "OSDE", FechaCreacion = DateTime.UtcNow };
        contexto.Pacientes.Add(paciente);
        await contexto.SaveChangesAsync();
        contexto.Turnos.Add(new Turno
        {
            PacienteId = paciente.Id, ProfesionalId = profesional.Id,
            Fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1), Horario = new TimeOnly(9, 0),
            Estado = EstadoTurno.Pendiente, FechaCreacion = DateTime.UtcNow
        });
        await contexto.SaveChangesAsync();

        await servicio.DesactivarAsync(profesional.Id, CancellationToken.None);
        await BackdatearDesactivacionAsync(contexto, profesional.Id);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.EliminarAsync(profesional.Id, CancellationToken.None));
    }

    // Pasados los 7 días y sin turnos asociados, la eliminación definitiva
    // borra tanto al profesional como su usuario de login.
    [Fact]
    public async Task EliminarAsync_DesactivadoHaceMasDeUnaSemanaYSinTurnos_BorraAlProfesionalYSuUsuario()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);

        await servicio.DesactivarAsync(profesional.Id, CancellationToken.None);
        await BackdatearDesactivacionAsync(contexto, profesional.Id);

        await servicio.EliminarAsync(profesional.Id, CancellationToken.None);

        Assert.Null(await contexto.Profesionales.FirstOrDefaultAsync(p => p.Id == profesional.Id));
        Assert.Null(await contexto.Usuarios.FirstOrDefaultAsync(u => u.ProfesionalId == profesional.Id));
    }

    // ActualizarHorariosAsync reemplaza por completo el horario: la segunda
    // llamada no debe "sumar" bloques a los de la primera, sino reemplazarlos.
    [Fact]
    public async Task ActualizarHorariosAsync_ReemplazaPorCompletoLosBloquesAnteriores()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto);
        var profesional = await CrearProfesionalConLoginAsync(contexto, servicio);

        await servicio.ActualizarHorariosAsync(profesional.Id, new ActualizarHorariosDto
        {
            Bloques = new List<BloqueHorarioDto>
            {
                new() { DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(12, 0) }
            }
        }, CancellationToken.None);

        var resultado = await servicio.ActualizarHorariosAsync(profesional.Id, new ActualizarHorariosDto
        {
            Bloques = new List<BloqueHorarioDto>
            {
                new() { DiaSemana = DayOfWeek.Tuesday, HoraInicio = new TimeOnly(14, 0), HoraFin = new TimeOnly(18, 0) }
            }
        }, CancellationToken.None);

        var bloque = Assert.Single(resultado.Horarios);
        Assert.Equal(DayOfWeek.Tuesday, bloque.DiaSemana);
    }

    // Mismo esquema de permisos que la duración de turno: un profesional no
    // puede tocar el horario de otro.
    [Fact]
    public async Task ActualizarHorariosAsync_ComoProfesionalSobreOtroProfesional_LanzaExcepcionProhibido()
    {
        using var contexto = CrearContexto();
        var servicioAdmin = CrearServicio(contexto);
        var profesionalPropio = await CrearProfesionalConLoginAsync(contexto, servicioAdmin, "propio");
        var profesionalAjeno = await CrearProfesionalConLoginAsync(contexto, servicioAdmin, "ajeno");

        var servicioProfesional = CrearServicio(contexto, RolUsuario.Profesional, profesionalPropio.Id);

        await Assert.ThrowsAsync<ExcepcionProhibido>(() => servicioProfesional.ActualizarHorariosAsync(
            profesionalAjeno.Id,
            new ActualizarHorariosDto { Bloques = new List<BloqueHorarioDto>() },
            CancellationToken.None));
    }

    private static async Task BackdatearDesactivacionAsync(ClinicaDbContext contexto, int profesionalId)
    {
        var profesional = await contexto.Profesionales.FirstAsync(p => p.Id == profesionalId);
        profesional.FechaDesactivacion = DateTime.UtcNow.AddDays(-8);
        await contexto.SaveChangesAsync();
    }
}
