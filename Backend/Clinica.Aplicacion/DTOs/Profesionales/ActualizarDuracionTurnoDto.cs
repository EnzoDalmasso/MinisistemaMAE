namespace Clinica.Aplicacion.DTOs.Profesionales;

// Usado tanto por el propio profesional como por el administrador para
// cambiar solo la duración de turno, sin tocar el resto de sus datos.
public class ActualizarDuracionTurnoDto
{
    public int DuracionTurnoMinutos { get; set; }
}
