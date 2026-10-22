using System;
using System.Collections.Generic;

namespace projeto_estoque_api.Models;

public partial class PedidoEntrega
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public string Cep { get; set; } = null!;

    public string Logradouro { get; set; } = null!;

    public string Numero { get; set; } = null!;

    public string? Complemento { get; set; }

    public string Bairro { get; set; } = null!;

    public string Cidade { get; set; } = null!;

    public string Uf { get; set; } = null!;

    public string? Observacao { get; set; }

    public virtual Pedido Pedido { get; set; } = null!;
}
