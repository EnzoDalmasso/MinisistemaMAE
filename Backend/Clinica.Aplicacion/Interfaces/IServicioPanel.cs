using Clinica.Aplicacion.DTOs.Panel;

namespace Clinica.Aplicacion.Interfaces;

public interface IServicioPanel
{
    Task<ResumenPanelDto> ObtenerResumenAsync(CancellationToken cancellationToken = default);
}
