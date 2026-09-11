using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Interfaces;

public interface IRepositorioProfesionales
{
    Task<List<Profesional>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Profesional?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Profesional profesional, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Profesional profesional, CancellationToken cancellationToken = default);

    // Borrado definitivo: solo se invoca tras verificar en el servicio que el
    // profesional está desactivado hace al menos 7 días y no tiene ningún
    // turno asociado (la FK Turno -> Profesional es Restrict a propósito).
    Task EliminarAsync(Profesional profesional, CancellationToken cancellationToken = default);

    // Reemplaza por completo el horario de atención del profesional (borra
    // los bloques existentes y carga los nuevos) en una sola operación, en
    // vez de andar reconciliando altas/bajas/modificaciones bloque por bloque.
    Task ReemplazarHorariosAsync(int profesionalId, List<BloqueHorarioProfesional> bloques, CancellationToken cancellationToken = default);
}
