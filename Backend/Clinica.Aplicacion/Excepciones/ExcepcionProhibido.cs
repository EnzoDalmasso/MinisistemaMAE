namespace Clinica.Aplicacion.Excepciones;

// Se traduce a 403 Forbidden. Se usa cuando un usuario autenticado intenta
// acceder a un recurso que no le pertenece (ej: un profesional pidiendo el
// turno de otro profesional por id).
public class ExcepcionProhibido : ExcepcionAplicacion
{
    public ExcepcionProhibido(string mensaje) : base(mensaje)
    {
    }
}
