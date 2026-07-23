// =============================================================================
// CAMADA: Infrastructure (Detalhes técnicos — depende de Application e Domain)
// ARQUIVO: SqliteRepository.cs
// =============================================================================
// CONCEITO — Implementação do Repository Pattern:
// Esta classe é a implementação concreta das interfaces IUsuarioRepository
// e IHistoricoRepository, ambas definidas na camada Application.
//
// Ela implementa DUAS interfaces ao mesmo tempo — isso é válido em C# e faz
// sentido aqui porque as operações de usuário e histórico compartilham o mesmo
// AppDbContext (mesma sessão com o banco).
//
// Por que implementar duas interfaces em uma única classe?
// Simplicidade: para um projeto pequeno, ter um único repositório que gerencia
// todas as entidades é prático. Em projetos maiores, separaria em classes
// distintas (UsuarioRepository e HistoricoRepository).
//
// A Application nunca vê "SqliteRepository" diretamente — ela só conhece
// IUsuarioRepository e IHistoricoRepository. O mapeamento acontece no Program.cs:
//   services.AddScoped<IUsuarioRepository, SqliteRepository>();
//   services.AddScoped<IHistoricoRepository, SqliteRepository>();
// =============================================================================

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

        // EnsureCreated() verifica se o banco de dados e todas as tabelas existem.
        // Se não existirem, os cria do zero com base no modelo definido no AppDbContext.
        // É a alternativa mais simples às "Migrations" do EF Core — adequada para
        // projetos que não precisam de versionamento de schema complexo.
        _context.Database.EnsureCreated();
    }

    // =========================================================================
    // Implementação de IUsuarioRepository
    // =========================================================================

    // Retorna todos os usuários. "ToListAsync()" executa a query SQL no banco
    // e materializa os resultados em uma lista na memória.
    public async Task<IEnumerable<Usuario?>> ObterUsuariosAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    // Busca um usuário pelo nome, ignorando maiúsculas/minúsculas.
    // FirstOrDefaultAsync retorna o primeiro resultado encontrado, ou null
    // se nenhum usuário corresponder ao filtro.
    //
    // LINQ traduzido para SQL: SELECT TOP 1 * FROM Usuarios WHERE LOWER(Nome) = LOWER(@nome)
    public async Task<Usuario?> ObterPorUsernameAsync(string nome)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Nome.ToLower() == nome.ToLower());
    }

    // Adiciona um novo usuário na sessão do EF Core e persiste no banco.
    // AddAsync() marca o objeto como "a ser inserido".
    // SaveChangesAsync() executa o INSERT SQL de fato.
    public async Task CadastrarUsuarioAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    // Remove um usuário pelo nome.
    // Note o padrão: busca primeiro, verifica se existe, então remove.
    // Isso evita exceções ao tentar remover algo que não existe.
    public async Task ExcluirUsuarioAsync(string nome)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Nome.ToLower() == nome.ToLower());

        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario); // Marca para remoção
            await _context.SaveChangesAsync(); // Executa o DELETE SQL
        }
    }

    // Verifica se existe ao menos um usuário no banco.
    // AnyAsync() é mais eficiente que Count() > 0 — gera "SELECT EXISTS(...)"
    // em vez de contar todos os registros.
    public async Task<bool> ExisteAlgumUsuarioAsync()
    {
        return await _context.Usuarios.AnyAsync();
    }

    // =========================================================================
    // Implementação de IHistoricoRepository
    // =========================================================================

    // Salva um registro de execução no banco.
    // O padrão é o mesmo do CadastrarUsuario: Add + SaveChanges.
    public async Task SalvarAsync(HistoricoExecucao historico)
    {
        await _context.Historicos.AddAsync(historico);
        await _context.SaveChangesAsync();
    }

    // Retorna todos os registros de histórico, do mais recente ao mais antigo.
    // OrderByDescending é traduzido para "ORDER BY DataExecucao DESC" no SQL.
    public async Task<IEnumerable<HistoricoExecucao>> ObterHistoricoAsync()
    {
        return await _context.Historicos
            .OrderByDescending(h => h.DataExecucao)
            .ToListAsync();
    }
}