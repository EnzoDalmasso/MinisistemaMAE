using Clinica.Aplicacion.DTOs.Turnos;
using Clinica.Aplicacion.Excepciones;
using Clinica.Aplicacion.Servicios;
using Clinica.Aplicacion.Validadores;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;
using Clinica.Infraestructura.Persistencia.Contexto;
using Clinica.Infraestructura.Persistencia.Repositorios;
using Clinica.Tests.Comunes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clinica.Tests.Servicios;

// Se usa SQLite en memoria (no el proveedor InMemory de EF) porque SQLite sí
// aplica restricciones únicas e índices filtrados de verdad, dándole fidelidad
// a la prueba de la regla de disponibilidad (que en producción corre sobre
// PostgreSQL, pero el mecanismo de "índice único parcial" es análogo).
public class ServicioTurnosTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<ClinicaDbContext> _opciones;

    public ServicioTurnosTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<ClinicaDbContext>().UseSqlite(_conexion).Options;

        using var contexto = new ClinicaDbContext(_opciones);
        contexto.Database.EnsureCreated();
    }

    public void Dispose() => _conexion.Dispose();

    private ClinicaDbContext CrearContexto() => new(_opciones);

    private static ServicioTurnos CrearServicio(ClinicaDbContext contexto, RolUsuario rol = RolUsuario.Administrador, int? profesionalId = null) => new(
        new RepositorioTurnos(contexto),
        new RepositorioPacientes(contexto),
        new RepositorioProfesionales(contexto),
        new UsuarioActualFalso(rol, profesionalId),
        new CrearTurnoDtoValidador(),
        new ActualizarTurnoDtoValidador());

    private static async Task<Paciente> AgregarPacienteAsync(ClinicaDbContext contexto, string nombre = "Julián", string apellido = "Ramírez")
    {
        var paciente = new Paciente { Nombre = nombre, Apellido = apellido, Telefono = "11-5555-0000", ObraSocial = "OSDE", FechaCreacion = DateTime.UtcNow };
        contexto.Pacientes.Add(paciente);
        await contexto.SaveChangesAsync();
        return paciente;
    }

    private static async Task<Profesional> AgregarProfesionalAsync(ClinicaDbContext contexto, string nombre = "Laura", string apellido = "Gómez", string especialidad = "Clínica Médica")
    {
        var profesional = new Profesional { Nombre = nombre, Apellido = apellido, Especialidad = especialidad, FechaCreacion = DateTime.UtcNow };
        contexto.Profesionales.Add(profesional);
        await contexto.SaveChangesAsync();
        return profesional;
    }

    private static async Task<(Paciente Paciente, Profesional Profesional)> SembrarPacienteYProfesionalAsync(ClinicaDbContext contexto) =>
        (await AgregarPacienteAsync(contexto), await AgregarProfesionalAsync(contexto));

    // 1. Crear un turno válido.
    [Fact]
    public async Task CrearAsync_ConDatosValidos_CreaElTurnoEnEstadoPendiente()
    {
        using var contexto = CrearContexto();
        var (paciente, profesional) = await SembrarPacienteYProfesionalAsync(contexto);
        var servicio = CrearServicio(contexto);

        var dto = new CrearTurnoDto
        {
            PacienteId = paciente.Id,
            ProfesionalId = profesional.Id,
            Fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1),
            Horario = new TimeOnly(10, 0)
        };

        var resultado = await servicio.CrearAsync(dto, CancellationToken.None);

        Assert.Equal(EstadoTurno.Pendiente, resultado.Estado);
        Assert.Equal(paciente.Id, resultado.PacienteId);
        Assert.Equal(profesional.Id, resultado.ProfesionalId);
    }

    // 2. Rechazar un turno duplicado (mismo profesional, fecha y horario).
    [Fact]
    public async Task CrearAsync_ConHorarioYaOcupadoPorElMismoProfesional_LanzaExcepcionConflicto()
    {
        using var contexto = CrearContexto();
        var (paciente, profesional) = await SembrarPacienteYProfesionalAsync(contexto);
        var servicio = CrearServicio(contexto);
        var fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var horario = new TimeOnly(10, 0);

        await servicio.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = horario }, CancellationToken.None);

        var otroPaciente = await AgregarPacienteAsync(contexto, "Sofía", "Fernández");

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.CrearAsync(
            new CrearTurnoDto { PacienteId = otroPaciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = horario },
            CancellationToken.None));
    }

    // Un turno cancelado libera el horario: debe poder reutilizarse sin conflicto.
    [Fact]
    public async Task CrearAsync_ConHorarioDeUnTurnoCancelado_NoLanzaConflicto()
    {
        using var contexto = CrearContexto();
        var (paciente, profesional) = await SembrarPacienteYProfesionalAsync(contexto);
        var servicio = CrearServicio(contexto);
        var fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var horario = new TimeOnly(10, 0);

        var primerTurno = await servicio.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = horario }, CancellationToken.None);
        await servicio.CancelarAsync(primerTurno.Id, CancellationToken.None);

        var otroPaciente = await AgregarPacienteAsync(contexto, "Lucas", "Torres");
        var nuevoTurno = await servicio.CrearAsync(new CrearTurnoDto { PacienteId = otroPaciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = horario }, CancellationToken.None);

        Assert.Equal(EstadoTurno.Pendiente, nuevoTurno.Estado);
    }

    // 3. Modificar un turno generando conflicto (mover un turno al horario de otro turno activo).
    [Fact]
    public async Task ActualizarAsync_MoviendoloAUnHorarioYaOcupado_LanzaExcepcionConflicto()
    {
        using var contexto = CrearContexto();
        var (paciente, profesional) = await SembrarPacienteYProfesionalAsync(contexto);
        var servicio = CrearServicio(contexto);
        var fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        var turnoOcupado = await servicio.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = new TimeOnly(9, 0) }, CancellationToken.None);
        var turnoAMover = await servicio.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = new TimeOnly(10, 0) }, CancellationToken.None);

        var dtoActualizar = new ActualizarTurnoDto
        {
            PacienteId = paciente.Id,
            ProfesionalId = profesional.Id,
            Fecha = fecha,
            Horario = new TimeOnly(9, 0), // mismo horario que turnoOcupado
            Estado = EstadoTurno.Confirmado
        };

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.ActualizarAsync(turnoAMover.Id, dtoActualizar, CancellationToken.None));

        // El turno original no debe haber cambiado tras el intento fallido.
        var sinCambios = await servicio.ObtenerPorIdAsync(turnoAMover.Id, CancellationToken.None);
        Assert.Equal(new TimeOnly(10, 0), sinCambios.Horario);
    }

    // Editar un turno sin cambiar su horario (por ejemplo, solo el estado) no debe
    // dispararse a sí mismo como conflicto.
    [Fact]
    public async Task ActualizarAsync_SoloCambiandoElEstado_NoLanzaConflictoConsigoMismo()
    {
        using var contexto = CrearContexto();
        var (paciente, profesional) = await SembrarPacienteYProfesionalAsync(contexto);
        var servicio = CrearServicio(contexto);
        var fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var horario = new TimeOnly(9, 0);

        var turno = await servicio.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesional.Id, Fecha = fecha, Horario = horario }, CancellationToken.None);

        var actualizado = await servicio.ActualizarAsync(turno.Id, new ActualizarTurnoDto
        {
            PacienteId = paciente.Id,
            ProfesionalId = profesional.Id,
            Fecha = fecha,
            Horario = horario,
            Estado = EstadoTurno.Confirmado
        }, CancellationToken.None);

        Assert.Equal(EstadoTurno.Confirmado, actualizado.Estado);
    }

    // 4. Un profesional solo puede consultar sus propios turnos.
    [Fact]
    public async Task ObtenerAsync_ComoProfesional_SoloDevuelveSusPropiosTurnos()
    {
        using var contexto = CrearContexto();
        var (paciente, profesionalUno) = await SembrarPacienteYProfesionalAsync(contexto);
        var profesionalDos = await AgregarProfesionalAsync(contexto, "Martín", "Pereyra", "Pediatría");

        var servicioAdmin = CrearServicio(contexto);
        var fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        await servicioAdmin.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesionalUno.Id, Fecha = fecha, Horario = new TimeOnly(9, 0) }, CancellationToken.None);
        await servicioAdmin.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesionalDos.Id, Fecha = fecha, Horario = new TimeOnly(11, 0) }, CancellationToken.None);

        // El filtro intenta pedir explícitamente los turnos del otro profesional:
        // el servicio debe ignorarlo y devolver solo los propios de todas formas.
        var servicioProfesionalUno = CrearServicio(contexto, RolUsuario.Profesional, profesionalUno.Id);
        var resultado = await servicioProfesionalUno.ObtenerAsync(new TurnoFiltroDto { ProfesionalId = profesionalDos.Id }, CancellationToken.None);

        Assert.Single(resultado);
        Assert.Equal(profesionalUno.Id, resultado[0].ProfesionalId);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ComoProfesionalSobreTurnoDeOtroProfesional_LanzaExcepcionProhibido()
    {
        using var contexto = CrearContexto();
        var (paciente, profesionalUno) = await SembrarPacienteYProfesionalAsync(contexto);
        var profesionalDos = await AgregarProfesionalAsync(contexto, "Carla", "Sosa", "Dermatología");

        var servicioAdmin = CrearServicio(contexto);
        var turnoDeOtro = await servicioAdmin.CrearAsync(new CrearTurnoDto
        {
            PacienteId = paciente.Id,
            ProfesionalId = profesionalDos.Id,
            Fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1),
            Horario = new TimeOnly(9, 0)
        }, CancellationToken.None);

        var servicioProfesionalUno = CrearServicio(contexto, RolUsuario.Profesional, profesionalUno.Id);

        await Assert.ThrowsAsync<ExcepcionProhibido>(() => servicioProfesionalUno.ObtenerPorIdAsync(turnoDeOtro.Id, CancellationToken.None));
    }

    // 5. El administrador puede consultar todos los turnos, de cualquier profesional.
    [Fact]
    public async Task ObtenerAsync_ComoAdministrador_DevuelveTurnosDeTodosLosProfesionales()
    {
        using var contexto = CrearContexto();
        var (paciente, profesionalUno) = await SembrarPacienteYProfesionalAsync(contexto);
        var profesionalDos = await AgregarProfesionalAsync(contexto, "Martín", "Pereyra", "Pediatría");

        var servicioAdmin = CrearServicio(contexto);
        var fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        await servicioAdmin.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesionalUno.Id, Fecha = fecha, Horario = new TimeOnly(9, 0) }, CancellationToken.None);
        await servicioAdmin.CrearAsync(new CrearTurnoDto { PacienteId = paciente.Id, ProfesionalId = profesionalDos.Id, Fecha = fecha, Horario = new TimeOnly(11, 0) }, CancellationToken.None);

        var resultado = await servicioAdmin.ObtenerAsync(new TurnoFiltroDto(), CancellationToken.None);

        Assert.Equal(2, resultado.Count);
    }

    // 6. Un estado fuera del enum EstadoTurno debe ser rechazado por el validador.
    [Fact]
    public void ActualizarTurnoDtoValidador_ConEstadoFueraDeRango_EsInvalido()
    {
        var validador = new ActualizarTurnoDtoValidador();
        var dto = new ActualizarTurnoDto
        {
            PacienteId = 1,
            ProfesionalId = 1,
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Horario = new TimeOnly(10, 0),
            Estado = (EstadoTurno)99
        };

        var resultado = validador.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(ActualizarTurnoDto.Estado));
    }

    [Fact]
    public async Task CrearAsync_ConPacienteInexistente_LanzaExcepcionNoEncontrado()
    {
        using var contexto = CrearContexto();
        var profesional = await AgregarProfesionalAsync(contexto);
        var servicio = CrearServicio(contexto);

        var dto = new CrearTurnoDto
        {
            PacienteId = 9999,
            ProfesionalId = profesional.Id,
            Fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1),
            Horario = new TimeOnly(10, 0)
        };

        await Assert.ThrowsAsync<ExcepcionNoEncontrado>(() => servicio.CrearAsync(dto, CancellationToken.None));
    }
}
