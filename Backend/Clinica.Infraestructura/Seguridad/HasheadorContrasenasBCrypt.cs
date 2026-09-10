using Clinica.Aplicacion.Interfaces;

namespace Clinica.Infraestructura.Seguridad;

// BCrypt genera un salt aleatorio por contraseña automáticamente y lo incluye
// en el hash resultante, así que no hace falta administrar salts por separado.
public class HasheadorContrasenasBCrypt : IHasheadorContrasenas
{
    public string Hashear(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena);

    public bool Verificar(string contrasena, string hash) => BCrypt.Net.BCrypt.Verify(contrasena, hash);
}
