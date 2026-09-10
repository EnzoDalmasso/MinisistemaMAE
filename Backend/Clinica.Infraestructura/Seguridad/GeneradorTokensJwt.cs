using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinica.Aplicacion.Interfaces;
using Clinica.Dominio.Entidades;
using Clinica.Infraestructura.Configuracion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinica.Infraestructura.Seguridad;

public class GeneradorTokensJwt : IGeneradorTokens
{
    private readonly ConfiguracionJwt _configuracion;

    public GeneradorTokensJwt(IOptions<ConfiguracionJwt> opciones)
    {
        _configuracion = opciones.Value;
    }

    public (string Token, DateTime ExpiraEn) Generar(Usuario usuario)
    {
        var expiraEn = DateTime.UtcNow.AddMinutes(_configuracion.ExpiracionMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (usuario.ProfesionalId.HasValue)
        {
            // Claim propio (no estándar): el backend lo usa para filtrar "mis
            // turnos" sin depender de ningún parámetro enviado por el cliente.
            claims.Add(new Claim("profesionalId", usuario.ProfesionalId.Value.ToString()));
        }

        if (usuario.PacienteId.HasValue)
        {
            // Análogo a profesionalId, para el rol Paciente.
            claims.Add(new Claim("pacienteId", usuario.PacienteId.Value.ToString()));
        }

        var claveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion.Clave));
        var credenciales = new SigningCredentials(claveSimetrica, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuracion.Emisor,
            audience: _configuracion.Audiencia,
            claims: claims,
            expires: expiraEn,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }
}
