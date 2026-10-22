using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class Pedido
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public DateTime Data { get; set; }

    public string Status { get; set; } = null!;

    public decimal ValorTotal { get; set; }

    public string? Observacao { get; set; }

    public virtual PedidoEntrega? PedidoEntrega { get; set; }

    public virtual ICollection<PedidoItem> PedidoItems { get; set; } = new List<PedidoItem>();

    public virtual Usuario Usuario { get; set; } = null!;
}
