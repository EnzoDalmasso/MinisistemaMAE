using Clinica.Aplicacion.DTOs.Panel;
using Clinica.Aplicacion.Interfaces;
using Clinica.Aplicacion.Servicios.Comun;
using Clinica.Dominio.Enumeraciones;
using Clinica.Dominio.Interfaces;

namespace Clinica.Aplicacion.Servicios;

public class ServicioPanel : IServicioPanel
{
    private readonly IRepositorioTurnos _repositorioTurnos;
    private readonly IUsuarioActual _usuarioActual;

    public ServicioPanel(IRepositorioTurnos repositorioTurnos, IUsuarioActual usuarioActual)
    {
        _repositorioTurnos = repositorioTurnos;
        _usuarioActual = usuarioActual;
    }

    public async Task<ResumenPanelDto> ObtenerResumenAsync(CancellationToken cancellationToken = default)
    {
        var esProfesional = _usuarioActual.Rol == RolUsuario.Profesional;
        var profesionalId = esProfesional ? _usuarioActual.ProfesionalId : null;

        var conteos = await _repositorioTurnos.ContarPorEstadoAsync(profesionalId, cancellationToken);

        var resumen = new ResumenPanelDto
        {
            TurnosPendientes = conteos.GetValueOrDefault(EstadoTurno.Pendiente),
            TurnosConfirmados = conteos.GetValueOrDefault(EstadoTurno.Confirmado),
            TurnosCancelados = conteos.GetValueOrDefault(EstadoTurno.Cancelado),
            TurnosAtendidos = conteos.GetValueOrDefault(EstadoTurno.Atendido)
        };
        resumen.TotalTurnos = conteos.Values.Sum();

        if (esProfesional && profesionalId.HasValue)
        {
            var proximos = await _repositorioTurnos.BuscarAsync(
                profesionalId,
                fecha: null,
                estado: null,
                fechaDesde: DateOnly.FromDateTime(DateTime.Now),
                tomar: 5,
                cancellationToken: cancellationToken);

            resumen.ProximosTurnos = proximos
                .Where(t => t.Estado != EstadoTurno.Cancelado)
                .Select(TurnoMapeador.ADto)
                .ToList();
        }

        return resumen;
    }
}
