using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class PromocaoItem
{
    public int Id { get; set; }

    public int PromocaoId { get; set; }

    public int ProdutoId { get; set; }

    public decimal PrecoTabela { get; set; }

    public virtual Produto Produto { get; set; } = null!;

    public virtual Promocao Promocao { get; set; } = null!;
}
