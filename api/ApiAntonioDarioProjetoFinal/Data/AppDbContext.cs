using ApiAntonioDarioProjetoFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiAntonioDarioProjetoFinal.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Utilizador> Utilizadores { get; set; }
    public DbSet<Produto>    Produtos      { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Utilizador ────────────────────────────────────────
        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Nome).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Role).HasDefaultValue("User");
        });

        // ── Produto ───────────────────────────────────────────
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.Property(p => p.Preco).HasColumnType("decimal(10,2)");
            entity.Property(p => p.Nome).HasMaxLength(150).IsRequired();
            entity.Property(p => p.SKU).HasMaxLength(50).IsRequired();
        });
    }
}