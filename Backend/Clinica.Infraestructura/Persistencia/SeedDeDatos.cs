using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;
using Clinica.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Clinica.Infraestructura.Persistencia;

// Carga datos ficticios de demostración. Es idempotente (si ya hay usuarios,
// no hace nada), así que puede ejecutarse en cada arranque sin duplicar datos.
// Todos los nombres, teléfonos y obras sociales son inventados para la demo.
public static class SeedDeDatos
{
    public static async Task EjecutarAsync(ClinicaDbContext contexto, IHasheadorContrasenas hasheador, IConfiguration configuracion)
    {
        if (await contexto.Usuarios.AnyAsync())
        {
            return;
        }

        // Las contraseñas de demostración se toman de configuración (variables de
        // entorno o appsettings) y solo caen a un valor por defecto si no se definieron,
        // para que el ambiente de desarrollo funcione "out of the box" sin secretos reales.
        var claveAdministrador = configuracion["DatosSemilla:ContrasenaAdministrador"] ?? "Admin123!";
        var claveProfesional = configuracion["DatosSemilla:ContrasenaProfesional"] ?? "Profesional123!";

        var ahora = DateTime.UtcNow;

        var profesionales = new List<Profesional>
        {
            new() { Nombre = "Laura", Apellido = "Gómez", Especialidad = "Clínica Médica", FechaCreacion = ahora },
            new() { Nombre = "Martín", Apellido = "Pereyra", Especialidad = "Pediatría", FechaCreacion = ahora },
            new() { Nombre = "Carla", Apellido = "Sosa", Especialidad = "Dermatología", FechaCreacion = ahora }
        };
        contexto.Profesionales.AddRange(profesionales);
        await contexto.SaveChangesAsync();

        var pacientes = new List<Paciente>
        {
            new() { Nombre = "Julián", Apellido = "Ramírez", Telefono = "11-5555-0001", ObraSocial = "OSDE", FechaCreacion = ahora },
            new() { Nombre = "Sofía", Apellido = "Fernández", Telefono = "11-5555-0002", ObraSocial = "Swiss Medical", FechaCreacion = ahora },
            new() { Nombre = "Lucas", Apellido = "Torres", Telefono = "11-5555-0003", ObraSocial = "Galeno", FechaCreacion = ahora },
            new() { Nombre = "Valentina", Apellido = "Díaz", Telefono = "11-5555-0004", ObraSocial = "IOMA", FechaCreacion = ahora },
            new() { Nombre = "Nicolás", Apellido = "Molina", Telefono = "11-5555-0005", ObraSocial = "PAMI", FechaCreacion = ahora }
        };
        contexto.Pacientes.AddRange(pacientes);
        await contexto.SaveChangesAsync();

        var usuarioAdministrador = new Usuario
        {
            NombreUsuario = "administrador",
            ContrasenaHash = hasheador.Hashear(claveAdministrador),
            Rol = RolUsuario.Administrador,
            FechaCreacion = ahora
        };

        // El usuario de demostración con rol Profesional queda vinculado al
        // primer profesional del seed, para poder mostrar el filtrado por dueño.
        var usuarioProfesional = new Usuario
        {
            NombreUsuario = "profesional",
            ContrasenaHash = hasheador.Hashear(claveProfesional),
            Rol = RolUsuario.Profesional,
            ProfesionalId = profesionales[0].Id,
            FechaCreacion = ahora
        };

        contexto.Usuarios.AddRange(usuarioAdministrador, usuarioProfesional);
        await contexto.SaveChangesAsync();

        var hoy = DateOnly.FromDateTime(DateTime.Now);

        var turnos = new List<Turno>
        {
            new() { PacienteId = pacientes[0].Id, ProfesionalId = profesionales[0].Id, Fecha = hoy.AddDays(1), Horario = new TimeOnly(9, 0), Estado = EstadoTurno.Pendiente, FechaCreacion = ahora },
            new() { PacienteId = pacientes[1].Id, ProfesionalId = profesionales[0].Id, Fecha = hoy.AddDays(1), Horario = new TimeOnly(10, 0), Estado = EstadoTurno.Confirmado, FechaCreacion = ahora },
            new() { PacienteId = pacientes[2].Id, ProfesionalId = profesionales[1].Id, Fecha = hoy.AddDays(2), Horario = new TimeOnly(11, 0), Estado = EstadoTurno.Pendiente, FechaCreacion = ahora },
            new() { PacienteId = pacientes[3].Id, ProfesionalId = profesionales[2].Id, Fecha = hoy.AddDays(3), Horario = new TimeOnly(15, 30), Estado = EstadoTurno.Confirmado, FechaCreacion = ahora },
            new() { PacienteId = pacientes[4].Id, ProfesionalId = profesionales[0].Id, Fecha = hoy.AddDays(-2), Horario = new TimeOnly(9, 0), Estado = EstadoTurno.Atendido, FechaCreacion = ahora },
            new() { PacienteId = pacientes[0].Id, ProfesionalId = profesionales[1].Id, Fecha = hoy.AddDays(-1), Horario = new TimeOnly(16, 0), Estado = EstadoTurno.Cancelado, FechaCreacion = ahora }
        };
        contexto.Turnos.AddRange(turnos);
        await contexto.SaveChangesAsync();
    }
}
