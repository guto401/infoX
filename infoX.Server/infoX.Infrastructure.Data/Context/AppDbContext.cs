using infoX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace infoX.Infrastructure.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aqui você "registra" a sua tabela no Entity Framework
        public DbSet<Machine> Machines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Se precisar de configurações extras (Fluent API), é aqui.
            // Por enquanto, as Data Annotations que colocamos já resolvem!
        }
    }
}