using Clinica.Aplicacion.Excepciones;

namespace Clinica.API.Middleware;

// Punto único de manejo de errores para toda la API: evita repetir try/catch
// en cada controlador y garantiza un formato de respuesta consistente.
// Las excepciones de Clinica.Aplicacion se traducen a su código HTTP correspondiente;
// cualquier otra excepción se trata como un error interno y no expone detalles al cliente.
public class ManejadorGlobalDeExcepciones
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorGlobalDeExcepciones> _logger;

    public ManejadorGlobalDeExcepciones(RequestDelegate siguiente, ILogger<ManejadorGlobalDeExcepciones> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception excepcion)
        {
            await ManejarExcepcionAsync(contexto, excepcion);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext contexto, Exception excepcion)
    {
        int codigoEstado;
        string mensaje;
        IDictionary<string, string[]>? errores = null;

        switch (excepcion)
        {
            case ExcepcionValidacion ex:
                codigoEstado = StatusCodes.Status400BadRequest;
                mensaje = ex.Message;
                errores = ex.Errores;
                break;
            case ExcepcionCredencialesInvalidas ex:
                codigoEstado = StatusCodes.Status401Unauthorized;
                mensaje = ex.Message;
                break;
            case ExcepcionProhibido ex:
                codigoEstado = StatusCodes.Status403Forbidden;
                mensaje = ex.Message;
                break;
            case ExcepcionNoEncontrado ex:
                codigoEstado = StatusCodes.Status404NotFound;
                mensaje = ex.Message;
                break;
            case ExcepcionConflicto ex:
                codigoEstado = StatusCodes.Status409Conflict;
                mensaje = ex.Message;
                break;
            default:
                codigoEstado = StatusCodes.Status500InternalServerError;
                mensaje = "Ocurrió un error inesperado. Intente nuevamente más tarde.";
                // Solo se loguea (con stack trace) el error no controlado; nunca se
                // expone información interna en la respuesta HTTP.
                _logger.LogError(excepcion, "Error no controlado procesando {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
                break;
        }

        contexto.Response.ContentType = "application/json";
        contexto.Response.StatusCode = codigoEstado;

        object cuerpo = errores is null ? new { mensaje } : new { mensaje, errores };
        await contexto.Response.WriteAsJsonAsync(cuerpo);
    }
}
