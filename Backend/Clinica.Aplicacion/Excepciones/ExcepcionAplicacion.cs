namespace Clinica.Aplicacion.Excepciones;

// Excepción base para todos los errores de negocio conocidos. El middleware
// global de la API captura las subclases y las traduce al código HTTP correspondiente,
// evitando repetir try/catch en cada controlador.
public abstract class ExcepcionAplicacion : Exception
{
    protected ExcepcionAplicacion(string mensaje) : base(mensaje)
    {
    }
}
