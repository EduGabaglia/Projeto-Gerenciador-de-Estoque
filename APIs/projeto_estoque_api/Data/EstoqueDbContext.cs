using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using projeto_estoque_api.Models;

namespace projeto_estoque_api.Data;

public partial class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<DadosEmpresa> DadosEmpresas { get; set; }

    public virtual DbSet<EnderecoUsuario> EnderecoUsuarios { get; set; }

    public virtual DbSet<LocalizacaoGalpao> LocalizacaoGalpaos { get; set; }

    public virtual DbSet<MovimentacaoEstoque> MovimentacaoEstoques { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<PedidoEntrega> PedidoEntregas { get; set; }

    public virtual DbSet<PedidoItem> PedidoItems { get; set; }

    public virtual DbSet<Produto> Produtos { get; set; }

    public virtual DbSet<Promocao> Promocaos { get; set; }

    public virtual DbSet<PromocaoItem> PromocaoItems { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categoria");

            entity.HasIndex(e => e.Nome, "UQ_Categoria_Nome").IsUnique();

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Descricao).HasMaxLength(255);
            entity.Property(e => e.Nome).HasMaxLength(80);
        });

        modelBuilder.Entity<DadosEmpresa>(entity =>
        {
            entity.ToTable("DadosEmpresa");

            entity.HasIndex(e => e.Cnpj, "UQ_DadosEmpresa_CNPJ").IsUnique();

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Bairro).HasMaxLength(100);
            entity.Property(e => e.Cep)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CEP");
            entity.Property(e => e.Cidade).HasMaxLength(100);
            entity.Property(e => e.Cnpj)
                .HasMaxLength(14)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CNPJ");
            entity.Property(e => e.Complemento).HasMaxLength(100);
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.Endereco).HasMaxLength(150);
            entity.Property(e => e.Nome).HasMaxLength(150);
            entity.Property(e => e.NomeFantasia).HasMaxLength(150);
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UF");
        });

        modelBuilder.Entity<EnderecoUsuario>(entity =>
        {
            entity.ToTable("EnderecoUsuario");

            entity.HasIndex(e => e.UsuarioId, "IX_EnderecoUsuario_UsuarioId");

            entity.Property(e => e.Apelido).HasMaxLength(50);
            entity.Property(e => e.Bairro).HasMaxLength(100);
            entity.Property(e => e.Cep)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CEP");
            entity.Property(e => e.Cidade).HasMaxLength(100);
            entity.Property(e => e.Complemento).HasMaxLength(100);
            entity.Property(e => e.Logradouro).HasMaxLength(150);
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UF");

            entity.HasOne(d => d.Usuario).WithMany(p => p.EnderecoUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EnderecoUsuario_Usuario");
        });

        modelBuilder.Entity<LocalizacaoGalpao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Localiza__3214EC079F8829F4");

            entity.ToTable("LocalizacaoGalpao");

            entity.Property(e => e.Localizacao)
                .HasMaxLength(35)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.ToTable("MovimentacaoEstoque");

            entity.HasIndex(e => e.Data, "IX_MovimentacaoEstoque_Data");

            entity.HasIndex(e => e.ProdutoId, "IX_MovimentacaoEstoque_ProdutoId");

            entity.Property(e => e.Data).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Observacao).HasMaxLength(500);
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Produto).WithMany(p => p.MovimentacaoEstoques)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimentacaoEstoque_Produto");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedido");

            entity.HasIndex(e => e.Data, "IX_Pedido_Data");

            entity.HasIndex(e => e.UsuarioId, "IX_Pedido_UsuarioId");

            entity.Property(e => e.Data).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Observacao).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDENTE");
            entity.Property(e => e.ValorTotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pedido_Usuario");
        });

        modelBuilder.Entity<PedidoEntrega>(entity =>
        {
            entity.ToTable("PedidoEntrega");

            entity.HasIndex(e => e.PedidoId, "UQ_PedidoEntrega_Pedido").IsUnique();

            entity.Property(e => e.Bairro).HasMaxLength(100);
            entity.Property(e => e.Cep)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CEP");
            entity.Property(e => e.Cidade).HasMaxLength(100);
            entity.Property(e => e.Complemento).HasMaxLength(100);
            entity.Property(e => e.Logradouro).HasMaxLength(150);
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Observacao).HasMaxLength(255);
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("UF");

            entity.HasOne(d => d.Pedido).WithOne(p => p.PedidoEntrega)
                .HasForeignKey<PedidoEntrega>(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PedidoEntrega_Pedido");
        });

        modelBuilder.Entity<PedidoItem>(entity =>
        {
            entity.ToTable("PedidoItem");

            entity.HasIndex(e => e.PedidoId, "IX_PedidoItem_PedidoId");

            entity.HasIndex(e => e.ProdutoId, "IX_PedidoItem_ProdutoId");

            entity.HasIndex(e => new { e.PedidoId, e.ProdutoId }, "UQ_PedidoItem_Pedido_Produto").IsUnique();

            entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Pedido).WithMany(p => p.PedidoItems)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PedidoItem_Pedido");

            entity.HasOne(d => d.Produto).WithMany(p => p.PedidoItems)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PedidoItem_Produto");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produto");

            entity.HasIndex(e => e.CategoriaId, "IX_Produto_CategoriaId");

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Nome).HasMaxLength(150);
            entity.Property(e => e.Preco).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Produtos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Produto_Categoria");

            entity.HasOne(d => d.Localizacao).WithMany(p => p.Produtos)
                .HasForeignKey(d => d.LocalizacaoId)
                .HasConstraintName("FK_Produto_LocalizacaoGalpao");
        });

        modelBuilder.Entity<Promocao>(entity =>
        {
            entity.ToTable("Promocao");

            entity.HasIndex(e => e.Nome, "UQ_Promocao_Nome").IsUnique();

            entity.Property(e => e.Ativa).HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DescontoPercentual).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Nome).HasMaxLength(150);
        });

        modelBuilder.Entity<PromocaoItem>(entity =>
        {
            entity.ToTable("PromocaoItem");

            entity.HasIndex(e => e.ProdutoId, "IX_PromocaoItem_ProdutoId");

            entity.HasIndex(e => new { e.PromocaoId, e.ProdutoId }, "UQ_PromocaoItem_Promocao_Produto").IsUnique();

            entity.Property(e => e.PrecoTabela).HasColumnType("decimal(9, 2)");

            entity.HasOne(d => d.Produto).WithMany(p => p.PromocaoItems)
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromocaoItem_Produto");

            entity.HasOne(d => d.Promocao).WithMany(p => p.PromocaoItems)
                .HasForeignKey(d => d.PromocaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromocaoItem_Promocao");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Email, "UQ_Usuario_Email").IsUnique();

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.Nome).HasMaxLength(120);
            entity.Property(e => e.SenhaHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
