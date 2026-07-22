// =============================================================================
// CAMADA: Infrastructure (Detalhes técnicos — depende de Application e Domain)
// ARQUIVO: AppDbContext.cs
// =============================================================================
// CONCEITO — Entity Framework Core (EF Core):
// O EF Core é um ORM (Object-Relational Mapper): ele traduz operações em objetos
// C# para comandos SQL, e resultados SQL de volta para objetos C#.
// Em vez de escrever "INSERT INTO Usuarios VALUES (...)", você escreve
// "_context.Usuarios.AddAsync(usuario)".
//
// CONCEITO — DbContext:
// O DbContext é a "sessão" com o banco de dados. Ele rastreia quais objetos
// foram modificados e gera o SQL necessário ao chamar SaveChangesAsync().
//
// CONCEITO — Por que isso fica na Infrastructure?
// O AppDbContext é um detalhe de implementação — podemos trocar o SQLite por
// PostgreSQL mudando apenas esta classe e os pacotes NuGet. As camadas Domain
// e Application nunca precisam saber que banco está sendo usado.
// =============================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class AppDbContext : DbContext
{
    // DbSet<T> representa uma tabela no banco de dados.
    // Através dele fazemos consultas LINQ que são traduzidas para SQL automaticamente.
    // "null!" diz ao compilador: "confie em mim, este campo nunca será null em runtime"
    // (o EF Core garante a inicialização quando o contexto é criado).
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<HistoricoExecucao> Historicos { get; set; } = null!;

    // Configura a string de conexão com o banco de dados.
    // Esta é uma configuração de infraestrutura pura.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ⚠️ ATENÇÃO — BUG DE CAMINHO RELATIVO:
        // "Data Source=infoX.db" cria o arquivo no "diretório de trabalho atual"
        // (Environment.CurrentDirectory), que pode ser diferente do diretório
        // do executável se o usuário iniciar o programa de outro lugar no terminal.
        //
        // O correto seria usar AppContext.BaseDirectory para sempre criar o banco
        // no mesmo diretório do .exe, consistente com a pasta Scripts/:
        //
        //   var dbPath = Path.Combine(AppContext.BaseDirectory, "infoX.db");
        //   optionsBuilder.UseSqlite($"Data Source={dbPath}");
        optionsBuilder.UseSqlite("Data Source=infoX.db");
    }

    // Configurações do modelo de dados — como as entidades mapeiam para tabelas.
    // Chamado pelo EF Core uma vez ao inicializar, antes de qualquer consulta.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Sempre chame o base primeiro

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            // Define Id como chave primária (o EF Core já inferiria por convenção,
            // mas ser explícito é uma boa prática de legibilidade).
            entity.HasKey(e => e.Id);

            // Cria um índice UNIQUE na coluna Nome — o banco rejeita dois usuários
            // com o mesmo nome, garantindo integridade no nível do banco de dados.
            entity.HasIndex(e => e.Nome).IsUnique();
        });

        // Configuração da entidade HistoricoExecucao
        modelBuilder.Entity<HistoricoExecucao>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Por padrão, o EF Core salvaria o StatusEnum como inteiro (0, 1, 2...).
            // HasConversion<string>() faz o EF salvar "Concluido", "Erro", etc.
            // Isso torna o banco legível por humanos sem precisar de um dicionário.
            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });
    }
}