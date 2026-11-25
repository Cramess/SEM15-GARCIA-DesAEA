using Gimnasio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Datos.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> BuscarPorCorreoAsync(string correo);
    Task<bool> ExisteCorreoAsync(string correo);
    Task AgregarAsync(Usuario usuario);
    Task GuardarAsync();
}

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly GimnasioContext _context;

    public UsuarioRepositorio(GimnasioContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> BuscarPorCorreoAsync(string correo) =>
        await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo == correo);

    public async Task<bool> ExisteCorreoAsync(string correo) =>
        await _context.Usuarios
            .AnyAsync(u => u.Correo == correo);

    public async Task AgregarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task GuardarAsync()
    {
        await _context.SaveChangesAsync();
    }
}
