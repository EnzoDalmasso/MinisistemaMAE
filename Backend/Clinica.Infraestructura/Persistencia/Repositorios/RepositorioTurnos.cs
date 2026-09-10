using Clinica.Aplicacion.Excepciones;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Enumeraciones;
using Clinica.Dominio.Interfaces;
using Clinica.Infraestructura.Persistencia.Configuraciones;
using Clinica.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Clinica.Infraestructura.Persistencia.Repositorios;

public class RepositorioTurnos : IRepositorioTurnos
{
    private readonly ClinicaDbContext _contexto;

    public RepositorioTurnos(ClinicaDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Turno?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _contexto.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Profesional)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<List<Turno>> BuscarAsync(
        int? profesionalId,
        DateOnly? fecha,
        EstadoTurno? estado,
        DateOnly? fechaDesde = null,
        int? tomar = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.Turnos
            .AsNoTracking()
            .Include(t => t.Paciente)
            .Include(t => t.Profesional)
            .AsQueryable();

        if (profesionalId.HasValue)
        {
            consulta = consulta.Where(t => t.ProfesionalId == profesionalId.Value);
        }

        if (fecha.HasValue)
        {
            consulta = consulta.Where(t => t.Fecha == fecha.Value);
        }

        if (estado.HasValue)
        {
            consulta = consulta.Where(t => t.Estado == estado.Value);
        }

        if (fechaDesde.HasValue)
        {
            consulta = consulta.Where(t => t.Fecha >= fechaDesde.Value);
        }

        consulta = consulta.OrderBy(t => t.Fecha).ThenBy(t => t.Horario);

        if (tomar.HasValue)
        {
            consulta = consulta.Take(tomar.Value);
        }

        return await consulta.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteSolapamientoAsync(
        int profesionalId,
        DateOnly fecha,
        TimeOnly horario,
        int? idExcluir,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.Turnos.AsNoTracking().Where(t =>
            t.ProfesionalId == profesionalId &&
            t.Fecha == fecha &&
            t.Horario == horario &&
            t.Estado != EstadoTurno.Cancelado);

        if (idExcluir.HasValue)
        {
            consulta = consulta.Where(t => t.Id != idExcluir.Value);
        }

        return await consulta.AnyAsync(cancellationToken);
    }

    public async Task<Dictionary<EstadoTurno, int>> ContarPorEstadoAsync(int? profesionalId, CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.Turnos.AsNoTracking().AsQueryable();

        if (profesionalId.HasValue)
        {
            consulta = consulta.Where(t => t.ProfesionalId == profesionalId.Value);
        }

        return await consulta
            .GroupBy(t => t.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Cantidad, cancellationToken);
    }

    public async Task AgregarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        _contexto.Turnos.Add(turno);
        await GuardarConTraduccionDeErroresAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        _contexto.Turnos.Update(turno);
        await GuardarConTraduccionDeErroresAsync(cancellationToken);
    }

    // Traduce errores de bajo nivel de PostgreSQL/EF Core a excepciones de Aplicación,
    // para que ServicioTurnos no necesite conocer tipos específicos del proveedor de datos.
    private async Task GuardarConTraduccionDeErroresAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Dos usuarios editaron el mismo turno al mismo tiempo (token xmin desactualizado).
            throw new ExcepcionConflicto("El turno fue modificado por otro usuario. Actualice la información e intente nuevamente.");
        }
        catch (DbUpdateException ex) when (EsViolacionDeDisponibilidad(ex))
        {
            // Red de seguridad final ante condiciones de carrera: dos requests pasaron
            // la verificación en memoria de ServicioTurnos casi al mismo tiempo, y el
            // índice único parcial de la base de datos rechazó el segundo insert/update.
            throw new ExcepcionConflicto("El profesional ya tiene un turno asignado para esa fecha y horario.");
        }
    }

    private static bool EsViolacionDeDisponibilidad(DbUpdateException ex) =>
        ex.InnerException is PostgresException postgresException &&
        postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
        postgresException.ConstraintName == TurnoConfiguracion.NombreIndiceDisponibilidad;
}
