using Clinica.Dominio.Entidades;

namespace Clinica.Aplicacion.Interfaces;

public interface IGeneradorTokens
{
    (string Token, DateTime ExpiraEn) Generar(Usuario usuario);
}
