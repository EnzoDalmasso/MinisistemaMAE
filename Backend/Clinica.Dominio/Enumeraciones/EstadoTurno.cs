namespace Clinica.Dominio.Enumeraciones;

// Estados permitidos para un turno. Esta es la única fuente de verdad sobre
// qué estados existen: el cliente nunca puede enviar un string arbitrario,
// siempre se valida contra este enum antes de persistir.
public enum EstadoTurno
{
    Pendiente = 1,
    Confirmado = 2,
    Cancelado = 3,
    Atendido = 4
}
