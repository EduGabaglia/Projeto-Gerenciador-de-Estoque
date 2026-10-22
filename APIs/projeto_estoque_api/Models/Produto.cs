using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class Produto
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public int CategoriaId { get; set; }

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public int Quantidade { get; set; }

    public bool Ativo { get; set; }

    public DateTime DataCadastro { get; set; }

    public int? LocalizacaoId { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual LocalizacaoGalpao? Localizacao { get; set; }

    public virtual ICollection<MovimentacaoEstoque> MovimentacaoEstoques { get; set; } = new List<MovimentacaoEstoque>();

    public virtual ICollection<PedidoItem> PedidoItems { get; set; } = new List<PedidoItem>();

    public virtual ICollection<PromocaoItem> PromocaoItems { get; set; } = new List<PromocaoItem>();
}
