using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class PedidoItem
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public virtual Pedido Pedido { get; set; } = null!;

    public virtual Produto Produto { get; set; } = null!;
}
