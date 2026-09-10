namespace Clinica.Aplicacion.Excepciones;

// Se traduce a 409 Conflict. Se usa puntualmente para la regla de disponibilidad
// de turnos (un profesional no puede tener dos turnos activos en el mismo horario).
public class ExcepcionConflicto : ExcepcionAplicacion
{
    public ExcepcionConflicto(string mensaje) : base(mensaje)
    {
    }
}
