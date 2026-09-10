namespace Clinica.Aplicacion.Comun;

// Horario de atención de la clínica. Es una validación simple a nivel global,
// no un sistema de horarios por profesional (fuera de alcance, ver mejoras
// futuras). Centralizado acá para no repetirlo en cada validador y en el
// cálculo de horarios disponibles.
public static class HorarioClinica
{
    public static readonly TimeOnly Apertura = new(7, 0);
    public static readonly TimeOnly Cierre = new(21, 0);
}
