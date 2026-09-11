namespace Clinica.Aplicacion.Comun;

// Horario general de la clínica: el techo/piso dentro del cual puede caer el
// horario propio de cada profesional (ver BloqueHorarioProfesional y
// ActualizarHorariosDtoValidador) y el horario por defecto para quien todavía
// no configuró el suyo (ver ServicioTurnos.ObtenerBloquesEfectivos).
// Centralizado acá para no repetirlo en cada validador.
public static class HorarioClinica
{
    public static readonly TimeOnly Apertura = new(7, 0);
    public static readonly TimeOnly Cierre = new(21, 0);
}
