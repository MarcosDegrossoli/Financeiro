using Financeiro.Models;
using Microsoft.EntityFrameworkCore;

namespace Financeiro.Data
{
    public class OfxDbContext : DbContext
    {
        // Este é o construtor que o EF Core espera para a injeção de dependência.
        public OfxDbContext(DbContextOptions<OfxDbContext> options)
            : base(options)
        {
        }

        public DbSet<Lancamento> Lancamentos { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Banco> Bancos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("financeiro");

            modelBuilder.Entity<Conta>()
                .HasOne(c => c.Banco)
                .WithMany(b => b.Contas)
                .HasForeignKey(c => c.BancoId);

            modelBuilder.Entity<Lancamento>()
                .HasOne(l => l.Conta)
                .WithMany(c => c.Lancamentos)
                .HasForeignKey(l => l.ContaId);
        }
    }
}