using ApiAntonioDarioProjetoFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAntonioDarioProjetoFinal.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Utilizador> Utilizadores { get; set; }
    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Utilizador>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Produto>()
            .HasIndex(p => p.SKU).IsUnique();
    }
}