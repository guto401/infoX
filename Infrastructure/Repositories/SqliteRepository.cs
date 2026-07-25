using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SqliteRepository : IUsuarioRepository, IHistoricoRepository
{
    private readonly AppDbContext _context;

    public SqliteRepository(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public async Task<IEnumerable<Usuario?>> ObterUsuariosAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> ObterPorUsernameAsync(string nome)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Nome.ToLower() == nome.ToLower());
    }

    public async Task CadastrarUsuarioAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirUsuarioAsync(string nome)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Nome.ToLower() == nome.ToLower());

        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteAlgumUsuarioAsync()
    {
        return await _context.Usuarios.AnyAsync();
    }

    public async Task SalvarAsync(HistoricoExecucao historico)
    {
        await _context.Historicos.AddAsync(historico);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<HistoricoExecucao>> ObterHistoricoAsync()
    {
        return await _context.Historicos
            .OrderByDescending(h => h.DataExecucao)
            .ToListAsync();
    }
}