using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class AppDbContext : DbContext
{
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<HistoricoExecucao> Historicos { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=infoX.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeamentos simples e limpos
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nome).IsUnique();
        });

        modelBuilder.Entity<HistoricoExecucao>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Salva o nosso StatusEnum como string no banco em vez de inteiro (melhor para leitura direta do banco)
            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });
    }
}