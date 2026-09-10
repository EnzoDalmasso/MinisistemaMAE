using Clinica.Aplicacion.DTOs.Turnos;

namespace Clinica.Aplicacion.DTOs.Panel;

// Contenido distinto según el rol: el administrador recibe conteos globales,
// el profesional recibe además sus próximos turnos. Se arma un único DTO con
// campos opcionales para no duplicar el endpoint.
public class ResumenPanelDto
{
    public int TotalTurnos { get; set; }
    public int TurnosPendientes { get; set; }
    public int TurnosConfirmados { get; set; }
    public int TurnosCancelados { get; set; }
    public int TurnosAtendidos { get; set; }

    public List<TurnoDto> ProximosTurnos { get; set; } = new();
}
