using Clinica.Dominio.Entidades;
using Clinica.Dominio.Interfaces;
using Clinica.Infraestructura.Persistencia.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infraestructura.Persistencia.Repositorios;

public class RepositorioUsuarios : IRepositorioUsuarios
{
    private readonly ClinicaDbContext _contexto;

    public RepositorioUsuarios(ClinicaDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) =>
        await _contexto.Usuarios
            .Include(u => u.Profesional)
            .Include(u => u.Paciente)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
