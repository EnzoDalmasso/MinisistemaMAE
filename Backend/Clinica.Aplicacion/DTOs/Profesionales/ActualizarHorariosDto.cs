namespace Clinica.Aplicacion.DTOs.Profesionales;

// Reemplaza por completo el horario de atención del profesional: la lista
// que llega acá es "la verdad" (se borra lo anterior y se carga esto), no un
// diff. Lista vacía es válida y significa "sin horario propio configurado"
// (ver ServicioTurnos, que en ese caso cae al horario general de la clínica).
public class ActualizarHorariosDto
{
    public List<BloqueHorarioDto> Bloques { get; set; } = new();
}
