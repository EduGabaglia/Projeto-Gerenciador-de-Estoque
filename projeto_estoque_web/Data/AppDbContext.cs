
using Microsoft.EntityFrameworkCore;
using projeto_estoque_web.Models;

namespace projeto_estoque_web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Categoria> Categorias => Set<Categoria>();

        public DbSet<LocalizacaoGalpao> LocalizacoesGalpao
            => Set<LocalizacaoGalpao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("Produto", "dbo");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Preco)
                    .HasColumnType("decimal(10,2)");

                // RELACIONAMENTO COM CATEGORIA
                entity.HasOne(p => p.Categoria)
                    .WithMany(c => c.Produtos)
                    .HasForeignKey(p => p.CategoriaId);

                // RELACIONAMENTO COM LOCALIZAÇÃO
                entity.HasOne(p => p.LocalizacaoGalpao)
                    .WithMany(l => l.Produtos)
                    .HasForeignKey(p => p.LocalizacaoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria", "dbo");

                entity.HasKey(c => c.Id);
            });

            modelBuilder.Entity<LocalizacaoGalpao>(entity =>
            {
                entity.ToTable("LocalizacaoGalpao", "dbo");

                entity.HasKey(l => l.Id);

                entity.Property(l => l.Localizacao)
                    .HasColumnType("varchar(35)")
                    .IsRequired();
            });
        }
    }
}
