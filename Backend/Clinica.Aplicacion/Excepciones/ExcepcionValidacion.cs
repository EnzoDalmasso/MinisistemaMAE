namespace Clinica.Aplicacion.Excepciones;

// Se lanza cuando los datos de entrada no cumplen las reglas de formato/obligatoriedad.
// Se traduce a 400 Bad Request. Agrupa los errores por campo para que el frontend
// pueda mostrarlos junto a cada input.
public class ExcepcionValidacion : ExcepcionAplicacion
{
    public IDictionary<string, string[]> Errores { get; }

    public ExcepcionValidacion(IDictionary<string, string[]> errores)
        : base("Los datos enviados no son válidos.")
    {
        Errores = errores;
    }

    public ExcepcionValidacion(string campo, string mensaje)
        : base("Los datos enviados no son válidos.")
    {
        Errores = new Dictionary<string, string[]> { [campo] = new[] { mensaje } };
    }
}
