namespace Clinica.Aplicacion.Excepciones;

// Se traduce a 404 Not Found.
public class ExcepcionNoEncontrado : ExcepcionAplicacion
{
    public ExcepcionNoEncontrado(string mensaje) : base(mensaje)
    {
    }
}
