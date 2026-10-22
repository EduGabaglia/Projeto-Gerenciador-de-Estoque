using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class MovimentacaoEstoque
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }

    public string Tipo { get; set; } = null!;

    public int Quantidade { get; set; }

    public DateTime Data { get; set; }

    public string? Observacao { get; set; }

    public virtual Produto Produto { get; set; } = null!;
}
