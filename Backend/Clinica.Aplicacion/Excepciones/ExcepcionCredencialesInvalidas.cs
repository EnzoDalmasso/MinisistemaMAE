namespace Clinica.Aplicacion.Excepciones;

// Se traduce a 401 Unauthorized. Mensaje deliberadamente genérico (no distingue
// "usuario no existe" de "contraseña incorrecta") para no filtrar qué usuarios existen.
public class ExcepcionCredencialesInvalidas : ExcepcionAplicacion
{
    public ExcepcionCredencialesInvalidas()
        : base("Usuario o contraseña incorrectos.")
    {
    }
}
