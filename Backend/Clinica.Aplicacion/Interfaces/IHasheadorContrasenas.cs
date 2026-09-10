namespace Clinica.Aplicacion.Interfaces;

public interface IHasheadorContrasenas
{
    string Hashear(string contrasena);
    bool Verificar(string contrasena, string hash);
}
