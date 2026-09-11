using Clinica.Aplicacion.DTOs.Pacientes;
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

public class ServicioPacientesTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<ClinicaDbContext> _opciones;

    public ServicioPacientesTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<ClinicaDbContext>().UseSqlite(_conexion).Options;

        using var contexto = new ClinicaDbContext(_opciones);
        contexto.Database.EnsureCreated();
    }

    public void Dispose() => _conexion.Dispose();

    private ClinicaDbContext CrearContexto() => new(_opciones);

    private static ServicioPacientes CrearServicio(
        ClinicaDbContext contexto, RolUsuario rol = RolUsuario.Paciente, int? pacienteId = null) => new(
        new RepositorioPacientes(contexto),
        new RepositorioUsuarios(contexto),
        new RepositorioTurnos(contexto),
        new UsuarioActualFalso(rol, pacienteId: pacienteId),
        new CrearPacienteDtoValidador(),
        new ActualizarPacienteDtoValidador(),
        new ActualizarContactoPacienteDtoValidador());

    private static async Task<Paciente> AgregarPacienteAutogestionadoAsync(ClinicaDbContext contexto, string dni = "30123456")
    {
        var paciente = new Paciente { Nombre = "Camila", Apellido = "Suárez", Dni = dni, FechaCreacion = DateTime.UtcNow };
        contexto.Pacientes.Add(paciente);
        await contexto.SaveChangesAsync();
        return paciente;
    }

    private static ActualizarPacienteDto DtoDeActualizacionPara(Paciente p, string? dni = null) => new()
    {
        Nombre = p.Nombre,
        Apellido = p.Apellido,
        Telefono = p.Telefono ?? "11-5555-0000",
        ObraSocial = p.ObraSocial ?? "OSDE",
        Dni = dni
    };

    // Un paciente autogestionado arranca sin teléfono/obra social/email;
    // completarlos (ej. al pedir su primer turno) los persiste en su ficha.
    [Fact]
    public async Task ActualizarContactoPropioAsync_CompletaLosDatosDeContactoDelPacienteAutenticado()
    {
        using var contexto = CrearContexto();
        var paciente = await AgregarPacienteAutogestionadoAsync(contexto);
        var servicio = CrearServicio(contexto, pacienteId: paciente.Id);

        var resultado = await servicio.ActualizarContactoPropioAsync(
            new ActualizarContactoPacienteDto { Telefono = "11-5555-0099", ObraSocial = "OSDE", Email = "camila@example.com" },
            CancellationToken.None);

        Assert.Equal("11-5555-0099", resultado.Telefono);
        Assert.Equal("OSDE", resultado.ObraSocial);
        Assert.Equal("camila@example.com", resultado.Email);

        var enBaseDeDatos = await contexto.Pacientes.FindAsync(paciente.Id);
        Assert.Equal("camila@example.com", enBaseDeDatos!.Email);
    }

    // Nunca debe poder actualizar el contacto de otro paciente: no hay id en
    // el DTO, así que la cuenta que llama siempre opera sobre sí misma.
    [Fact]
    public async Task ActualizarContactoPropioAsync_SinPacienteAsociado_LanzaProhibido()
    {
        using var contexto = CrearContexto();
        var servicio = CrearServicio(contexto, pacienteId: null);

        await Assert.ThrowsAsync<ExcepcionProhibido>(() => servicio.ActualizarContactoPropioAsync(
            new ActualizarContactoPacienteDto { Telefono = "11-5555-0099", ObraSocial = "OSDE", Email = "camila@example.com" },
            CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarContactoPropioAsync_ConEmailInvalido_LanzaValidacion()
    {
        using var contexto = CrearContexto();
        var paciente = await AgregarPacienteAutogestionadoAsync(contexto);
        var servicio = CrearServicio(contexto, pacienteId: paciente.Id);

        await Assert.ThrowsAsync<ExcepcionValidacion>(() => servicio.ActualizarContactoPropioAsync(
            new ActualizarContactoPacienteDto { Telefono = "11-5555-0099", ObraSocial = "OSDE", Email = "no-es-un-email" },
            CancellationToken.None));
    }

    // El administrador puede corregir un DNI mal cargado por el paciente al autogestionarse.
    [Fact]
    public async Task ActualizarAsync_ConDniValido_LoActualiza()
    {
        using var contexto = CrearContexto();
        var paciente = await AgregarPacienteAutogestionadoAsync(contexto, dni: "30123456");
        var servicio = CrearServicio(contexto);

        var resultado = await servicio.ActualizarAsync(paciente.Id, DtoDeActualizacionPara(paciente, dni: "30123457"), CancellationToken.None);

        Assert.Equal("30123457", resultado.Dni);
    }

    [Fact]
    public async Task ActualizarAsync_ConDniYaUsadoPorOtroPaciente_LanzaConflicto()
    {
        using var contexto = CrearContexto();
        var pacienteA = await AgregarPacienteAutogestionadoAsync(contexto, dni: "30123456");
        var pacienteB = await AgregarPacienteAutogestionadoAsync(contexto, dni: "30123457");
        var servicio = CrearServicio(contexto);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.ActualizarAsync(
            pacienteB.Id, DtoDeActualizacionPara(pacienteB, dni: pacienteA.Dni), CancellationToken.None));
    }

    [Fact]
    public async Task EliminarAsync_ConTurnosAsociados_LanzaConflicto()
    {
        using var contexto = CrearContexto();
        var paciente = await AgregarPacienteAutogestionadoAsync(contexto);
        var profesional = new Profesional { Nombre = "Laura", Apellido = "Gómez", Especialidad = "Clínica Médica", FechaCreacion = DateTime.UtcNow };
        contexto.Profesionales.Add(profesional);
        await contexto.SaveChangesAsync();
        contexto.Turnos.Add(new Turno
        {
            PacienteId = paciente.Id, ProfesionalId = profesional.Id,
            Fecha = DateOnly.FromDateTime(DateTime.Now).AddDays(1), Horario = new TimeOnly(9, 0),
            Estado = EstadoTurno.Pendiente, FechaCreacion = DateTime.UtcNow
        });
        await contexto.SaveChangesAsync();
        var servicio = CrearServicio(contexto);

        await Assert.ThrowsAsync<ExcepcionConflicto>(() => servicio.EliminarAsync(paciente.Id, CancellationToken.None));
    }

    // Al eliminar un paciente autogestionado hay que borrar también su
    // Usuario vinculado (la FK es Restrict) para que el borrado no falle.
    [Fact]
    public async Task EliminarAsync_SinTurnos_BorraAlPacienteYSuUsuarioVinculado()
    {
        using var contexto = CrearContexto();
        var paciente = await AgregarPacienteAutogestionadoAsync(contexto);
        contexto.Usuarios.Add(new Usuario
        {
            NombreUsuario = paciente.Dni!,
            ContrasenaHash = "hash-de-prueba",
            Rol = RolUsuario.Paciente,
            PacienteId = paciente.Id,
            FechaCreacion = DateTime.UtcNow
        });
        await contexto.SaveChangesAsync();
        var servicio = CrearServicio(contexto);

        await servicio.EliminarAsync(paciente.Id, CancellationToken.None);

        Assert.Null(await contexto.Pacientes.FindAsync(paciente.Id));
        Assert.Null(await contexto.Usuarios.FirstOrDefaultAsync(u => u.PacienteId == paciente.Id));
    }
}
