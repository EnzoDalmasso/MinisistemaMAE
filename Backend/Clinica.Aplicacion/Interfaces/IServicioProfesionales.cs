using Clinica.Aplicacion.DTOs.Profesionales;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioProfesionales
{
    Task<List<ProfesionalDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<ProfesionalDto> CrearAsync(CrearProfesionalDto dto, CancellationToken cancellationToken = default);
    Task<ProfesionalDto> ActualizarAsync(int id, ActualizarProfesionalDto dto, CancellationToken cancellationToken = default);

    // Puede llamarlo el Administrador (sobre cualquier profesional) o el
    // propio profesional (solo sobre sí mismo); el chequeo de permiso vive acá.
    Task<ProfesionalDto> ActualizarDuracionTurnoAsync(int id, ActualizarDuracionTurnoDto dto, CancellationToken cancellationToken = default);

    // Mismo esquema de permisos que ActualizarDuracionTurnoAsync. Reemplaza
    // por completo los días/horarios de atención del profesional.
    Task<ProfesionalDto> ActualizarHorariosAsync(int id, ActualizarHorariosDto dto, CancellationToken cancellationToken = default);

    // Las tres siguientes son exclusivas del Administrador (ver
    // ProfesionalesController). Desactivar también desactiva el Usuario
    // vinculado (no puede loguearse) y deja de ofrecerse para turnos nuevos;
    // Reactivar revierte ambas cosas. Eliminar es definitivo y solo procede
    // si está desactivado hace 7+ días y no tiene ningún turno asociado.
    Task<ProfesionalDto> DesactivarAsync(int id, CancellationToken cancellationToken = default);
    Task<ProfesionalDto> ReactivarAsync(int id, CancellationToken cancellationToken = default);
    Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
